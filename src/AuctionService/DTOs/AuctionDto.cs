using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionService.DTOs
{
	public class AuctionDto
	{
		//remove all default values and ?
		//a DTO just should be a collection of properties effectively
		public Guid Id { get; set; }
		public int ReservePrice { get; set; } = 0;
		public string Seller { get; set; }
		public string Winner { get; set; }
		public int SoldAmount { get; set; }
		public int CurrentHighBid { get; set; }
		public DateTime CreateAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public DateTime AuctionEnd { get; set; }
		public string Status { get; set; }
		public string Make { get; set; }
		public string Model { get; set; }
		public int Year { get; set; }
		public string Color { get; set; }
		public int Mileage { get; set; }
		public string ImageUrl { get; set; }
	}
}