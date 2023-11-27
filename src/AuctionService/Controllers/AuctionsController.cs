using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuctionService.Controllers
{
	//more about ApiController: https://code-maze.com/apicontroller-attribute-in-asp-net-core-web-api/
	[ApiController]
	[Route("api/auctions")]
	public class AuctionsController : ControllerBase
	{
		private readonly AuctionDbContext _context;
		private readonly IMapper _mapper;

		public AuctionsController(AuctionDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
		{
			var auctions = await _context.Auctions
							.Include(x => x.Item)
							.OrderBy(x => x.Item.Make)
							.ToListAsync();

			return _mapper.Map<List<AuctionDto>>(auctions);
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


		[HttpPost]
		public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
		{
			var auction = _mapper.Map<Auction>(auctionDto);
			//TODO: add current user as Seller
			auction.Seller = "test";

			_context.Auctions.Add(auction);

			//SaveChangesAsync trả về một số int, nếu nó không save gì vào db thì trả về 0
			//nếu nó trả về 0 thì result == false (DB không save gì mới)
			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not save changes to the DB");

			//CreatedAtAction return 201 code, a clear response body and the URL led to the new created resource in the response header
			return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDto>(auction));
		}

		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
		{
			var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

			if (auction == null) return NotFound();

			//TODO: check seller == username

			//??  returns the value of its left-hand operand if it isn't nul and vice versa
			auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
			auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
			auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
			auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
			auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Problem saving changes");

			return Ok();
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteAuction(Guid id)
		{
			var auction = await _context.Auctions.FindAsync(id);

			if (auction == null) return NotFound();

			//TODO: check username == seller
			_context.Auctions.Remove(auction);

			var result = await _context.SaveChangesAsync() > 0;

			if (!result) return BadRequest("Could not update DB");

			return Ok();
		}
	}
}