using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
	//more about ApiController: https://code-maze.com/apicontroller-attribute-in-asp-net-core-web-api/
	[ApiController]
	[Route("api/auctions")]
	public class AuctionsController : ControllerBase
	{
		private readonly AuctionDbContext _context;
		private readonly IMapper _mapper;
		private readonly IPublishEndpoint _publishEndpoint;

		//Inject IPublishEndpoint from MassTransit to allow us to publish the message
		public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
		{
			_context = context;
			_mapper = mapper;
			_publishEndpoint = publishEndpoint;
		}

		[HttpGet]
		public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
		{
			//AsQueryable so that query will have IQueryable type
			//so we can make more queries that we can if it had IOrderedQueryable type
			var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

			if (!string.IsNullOrEmpty(date))
			{
				query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
			}

			// var auctions = await _context.Auctions
			// 				.Include(x => x.Item)
			// 				.OrderBy(x => x.Item.Make)
			// 				.ToListAsync();

			// return _mapper.Map<List<AuctionDto>>(auctions);

			//ở trên là ToListAsync thành type list Auction rồi mới map vào Dto bằng Map
			//ở dưới này là map type queryable vào Dto bằng ProjectTo rồi mới ToListAsync
			//ConfigurationProvider để get mapping profile trong AutoMapper Service đã registered trong Program.cs
			//more about ProjectTo: https://docs.automapper.org/en/stable/Queryable-Extensions.html
			return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
		{
			var auction = await _context.Auctions
							.Include(x => x.Item)
							.FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			return _mapper.Map<AuctionDto>(auction);
		}


		[Authorize] //if anonymous users try to access this api they will get 401	
		[HttpPost]
		public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
		{
			var auction = _mapper.Map<Auction>(auctionDto);
			//TODO: add current user as Seller
			auction.Seller = User.Identity.Name; //with what we define in options of addjwtbearer(Program.cs), it will return the username of current user

			_context.Auctions.Add(auction);

			//move đống này lên trên SaveChangesAsync vì đống này là transaction
			//khi publish event thì nó cũng add event vào table (Inbox Outbox gì đó) trong DB luôn
			//nên SaveChangesAsync sẽ save cả auction và event
			//nếu lỗi cái nào thì rollback hết
			var newAuction = _mapper.Map<AuctionDto>(auction);
			//map the newAuction to AuctionCreated event model then publish the message(event)
			await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

			//SaveChangesAsync trả về một số int, nếu nó không save gì vào db thì trả về 0
			//nếu nó trả về 0 thì result == false (DB không save gì mới)
			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not save changes to the DB");

			//CreatedAtAction return 201 code, a clear response body and the URL led to the new created resource in the response header
			return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, newAuction);
		}

		[Authorize]
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
		{
			var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			//TODO: check seller == username
			if (auction.Seller != User.Identity.Name) return Forbid();

			//??  returns the value of its left-hand operand if it isn't nul and vice versa
			auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
			auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
			auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
			auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
			auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

			await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Problem saving changes");

			return Ok();
		}

		[Authorize]
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteAuction(Guid id)
		{
			var auction = await _context.Auctions.FindAsync(id);

			if (auction == null) return NotFound();

			//TODO: check username == seller
			if (auction.Seller != User.Identity.Name) return Forbid();

			_context.Auctions.Remove(auction);

			await _publishEndpoint.Publish<AuctionDeleted>(new { id = auction.Id.ToString() });

			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not update DB");

			return Ok();
		}
	}
}