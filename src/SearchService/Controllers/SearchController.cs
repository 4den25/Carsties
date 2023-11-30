using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers
{
	[ApiController]
	[Route("api/Search")]
	public class SearchController : ControllerBase
	{
		//we don't need to provide a constructor
		//we don't need to inject the MongoDb entities
		//we can just use it because it's a static class

		[HttpGet]
		//dùng ActionResult thì có thể return type ví dụ: ActionResult<Item>, còn IActionResult thì không
		// public async Task<ActionResult<List<Item>>> SearchItems(string searchTerm, int pageNumber = 1, int pageSize = 4)

		//if we pass an object as a param, it's going to look for this in the body of the request, 
		//not the query string params,
		//we have to tell it where to look for this
		public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
		{
			//read more at:https://mongodb-entities.com/api/MongoDB.Entities.Find-1.html
			//var query = DB.Find<Item>(); //để dùng pagination thì phải đổi từ Find sang PageSearch
			var query = DB.PagedSearch<Item, Item
			>();

			if (!string.IsNullOrEmpty(searchParams.SearchTerm))
			{
				//có 2 searchType là fuzzy và full, fuzzy nặng performance nên chỉ dùng khi cần
				//read more at: https://stackoverflow.com/questions/60397698/what-exactly-differs-fuzzy-search-from-full-text-search
				query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
			}

			//do chỗ này query = query.Sort...etc nên type của query mới từ PageSearch<Item> -> PageSearch<Item, Item>
			//dunno why we had to assign query to itself here?
			query = searchParams.OrderBy switch
			{
				"make" => query.Sort(x => x.Ascending(a => a.Make)),
				"new" => query.Sort(x => x.Descending(a => a.CreateAt)),
				_ => query.Sort(x => x.Ascending(a => a.AuctionEnd)) //this is default case
			};

			query = searchParams.FilterBy switch
			{
				"finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
				"endingSoon" => query.Match(x => x.AuctionEnd > DateTime.UtcNow
					&& x.AuctionEnd < DateTime.UtcNow.AddHours(6)), //trong 6 tiếng nữa sẽ kết thúc là endingSoon
				_ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
			};

			if (!string.IsNullOrEmpty(searchParams.Seller))
			{
				query.Match(x => x.Seller == searchParams.Seller);
			}

			if (!string.IsNullOrEmpty(searchParams.Winner))
			{
				query.Match(x => x.Winner == searchParams.Winner);
			}

			query.PageNumber(searchParams.PageNumber);
			query.PageSize(searchParams.PageSize);

			var result = await query.ExecuteAsync();

			return Ok(new
			{
				results = result.Results,
				pageCount = result.PageCount,
				totalCount = result.TotalCount
			});
		}
	}
}