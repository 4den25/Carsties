using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
	public class AuctionSvcHttpClient
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _config;

		public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
		{
			_httpClient = httpClient; //need this to call to AuctionService
			_config = config;
		}

		public async Task<List<Item>> GetItemsForSearchDb()
		{
			var lastUpdated = await DB.Find<Item, string>() //phải có string ở đây thì mới return string được
				.Sort(x => x.Descending(a => a.UpdatedAt))
				.Project(x => x.UpdatedAt.ToString()) //use .Project() to get back only the properties you need
				.ExecuteFirstAsync();

			Console.WriteLine("this is lastupdated:");
			Console.WriteLine(lastUpdated);
			//chỗ này sau khi drop DB thì lần đầu call method này last update = null
			//bên AuctionsController đã defined là nếu param date truyền từ query null thì khỏi query mà get all
			//này là để mỗi khi run lại application, auction nào đã seed vào DB nó sẽ không seed lại nữa			

			//call to AuctionService
			//GetFromJsonAsync is used to automatically deserialize the JSON we get back from AuctionService
			//AuctionServiceUrl is the config in appsettings.Development.json file
			return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"]
				+ "/api/auctions?date=" + lastUpdated);
		}
	}
}