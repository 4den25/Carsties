using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using ZstdSharp.Unsafe;

namespace SearchService.Consumers
{
	public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
	{
		private readonly IMapper _mapper;

		public AuctionUpdatedConsumer(IMapper mapper)
		{
			_mapper = mapper;
		}
		public async Task Consume(ConsumeContext<AuctionUpdated> context)
		{
			Console.WriteLine("--> Consuming auction updated " + context.Message.Id);

			var item = _mapper.Map<Item>(context.Message);

			var result = await DB.Update<Item>()
				.Match(x => x.ID == context.Message.Id)
				.ModifyOnly(a => new
				{
					a.Make,
					a.Model,
					a.Color,
					a.Year,
					a.Mileage
				}, item)
				.ExecuteAsync();

			if (!result.IsAcknowledged)
			{
				throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
			}
		}
	}
}