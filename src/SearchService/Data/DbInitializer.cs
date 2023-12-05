using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data
{
	public class DbInitializer
	{
		public static async Task InitDb(WebApplication app)
		{
			//initialize mongodb database
			await DB.InitAsync("SearchDb",
			MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

			//create index for Item for the certain fields that we want to be able to search on
			await DB.Index<Item>()
				.Key(x => x.Make, KeyType.Text)
				.Key(x => x.Model, KeyType.Text)
				.Key(x => x.Color, KeyType.Text)
				.CreateAsync();

			//check if we have any Item
			// var count = await DB.CountAsync<Item>();

			// if (count == 0)
			// {
			// 	Console.WriteLine("No data - wil attemp to seed");
			// 	var itemData = await File.ReadAllTextAsync("Data/auctions.json");

			// 	var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

			// 	//convert the json into a list of Item in .NET format
			// 	var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

			// 	await DB.SaveAsync(items);
			// }

			//when we run DbInitializer, we are not able to inject any thing to it
			//cuz it runs before the application
			//so we need to use scope
			using var scope = app.Services.CreateScope();
			var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

			var items = await httpClient.GetItemsForSearchDb();

			Console.WriteLine(items.Count + " returned from the auction service");
			Console.WriteLine(items[0].UpdatedAt);

			if (items.Count > 0) await DB.SaveAsync(items);
		}
	}
}