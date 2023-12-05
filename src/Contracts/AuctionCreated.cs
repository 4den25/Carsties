using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//dùng transmit thì bắt buộc event model được dùng giữa các service phải ở trong namespace giống nhau
//ở đây tạo hẳn một class library rồi tạo event model ở trỏng cho các service dùng chung
//nên it's not a problem
namespace Contracts
{
	public class AuctionCreated
	{
		public Guid Id { get; set; }
		public int ReservePrice { get; set; }
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