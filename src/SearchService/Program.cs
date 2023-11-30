
using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//register AuctionSvcHttpClient as a service
//1.Create an instance of HttpClient.
//2.Create an instance of AuctionSvcHttpClient, passing in the instance of HttpClient to its constructor.
//read more at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-8.0
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

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