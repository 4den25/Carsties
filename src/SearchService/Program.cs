
using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//register AuctionSvcHttpClient as a service
//1.Create an instance of HttpClient.
//2.Create an instance of AuctionSvcHttpClient, passing in the instance of HttpClient to its constructor.
//read more at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-8.0
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
	//need to tell masstransmit where to find the consumer
	x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

	//format the name of the queue
	//cuz perhaps there are many services having AuctionCreatedConsumer
	//in that case we have to named it somehow so we can distinguish them
	//search is the prefix, KebabCase gonna add the dash (-) between word
	//false means we dont wanna include the name of namespace
	x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
		{
			host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
			host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
		});
		//config retry policy cho riêng AuctionCreatedConsumer của queue search-auction-created
		//read more at: https://masstransit.io/documentation/configuration/consumers
		cfg.ReceiveEndpoint("search-auction-created", e =>
		{
			//read more at: https://masstransit.io/documentation/concepts/exceptions
			e.UseMessageRetry(r => r.Interval(5, 5));
			e.ConfigureConsumer<AuctionCreatedConsumer>(context);
		});

		//automatically configure a receive endpoint for the consumer
		//that we add above by AddConsumersFromNamespaceContaining
		//read more at: https://masstransit.io/documentation/configuration
		cfg.ConfigureEndpoints(context);
	});
});


var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//nếu auction service down thì nó sẽ liên tục gọi đi gọi lại
//kết quả là không chạy tới dòng app.Run(); được
//nên bỏ đoạn này vào lifetime để search service có thể chạy được dù auction service đang down
//khối try catch will be runned once the application starts
app.Lifetime.ApplicationStarted.Register(async () =>
{
	//chạy DbInitalizer trước khi run app
	try
	{
		await DbInitializer.InitDb(app);
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
	}
});

app.Run();

//nếu gọi tới auction service không được (HandleTransientHttpError) thì cứ 3s nó sẽ gọi lại 1 lần
//OrResult là thêm vào trường hợp mà nó sẽ gọi lại
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
	=> HttpPolicyExtensions
		.HandleTransientHttpError()
		.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
		.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));