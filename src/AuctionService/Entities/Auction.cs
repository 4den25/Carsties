using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuctionService.Entities
{
	public class Auction
	{
		public Guid Id { get; set; }
		public int ReservePrice { get; set; } = 0;
		public string Seller { get; set; }
		public string Winner { get; set; }
		public int? SoldAmount { get; set; }
		public int? CurrentHighBid { get; set; }
		public DateTime CreateAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime AuctionEnd { get; set; }

		//Navigation properties
		public Status Status { get; set; }
		public Item Item { get; set; }
	}
}