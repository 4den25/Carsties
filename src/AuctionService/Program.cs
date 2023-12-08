using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
	opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//specify the location of where our mapping profiles are
//it's going to take a look for any classes that derived from the Profile class 
//and register the mappings in memory so when it comes to using AutoMapper it's already good to go
//cách khác là dùng typeof(MappingProfiles) cũng được
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x =>
{
	x.AddEntityFrameworkOutbox<AuctionDbContext>(options =>
	{
		//if the service bus it not avaible,
		//every 10s, it's gonna attempt to look inside our outbox and see if there's anything
		//that hasn't been delivered yet once the service is avaiable
		options.QueryDelay = TimeSpan.FromSeconds(10);

		//we tell it which database provider we want to use
		options.UsePostgres();

		// enable the bus outbox
		options.UseBusOutbox();
	});

	x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.ConfigureEndpoints(context);
	});
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options => //pass the option we need for how we gonna validate this token from this resource server
	{
		//Authority tell the resource server who issues the token
		//then it can use its configuration, etc... and it will take the token and validate it with identity server
		options.Authority = builder.Configuration["IdentityServiceUrl"];
		options.RequireHttpsMetadata = false; //cuz our identity server is running on http
		options.TokenValidationParameters.ValidateAudience = false;
		options.TokenValidationParameters.NameClaimType = "username"; //này để get value của username claim trong token (Username của user), dùng trong API controller
	});

var app = builder.Build();

// Configure the HTTP request pipeline.

//need to use this before UseAuthorization or we will get 401
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

//chạy DbInitalizer trước khi run app
try
{
	DbInitializer.InitDb(app);
}
catch (Exception e)
{

	Console.WriteLine(e);
}

app.Run();
