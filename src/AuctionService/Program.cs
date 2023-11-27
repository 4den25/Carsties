using AuctionService.Data;
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

var app = builder.Build();

// Configure the HTTP request pipeline.



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
