using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers
{
	//các class consumer luôn có Consumer ở cuối tên vì đó là convention code của MassTransit
	public class AuctionCreatedConsumer : IConsumer<AuctionCreated> //this take the type of thing we are consuming
	{
		private readonly IMapper _mapper;

		public AuctionCreatedConsumer(IMapper mapper)
		{
			_mapper = mapper;
		}
		public async Task Consume(ConsumeContext<AuctionCreated> context)
		{
			Console.WriteLine("--> Consuming auction createad " + context.Message.Id);

			var item = _mapper.Map<Item>(context.Message);

			//an example of handling the fault in consumer
			//ném ra exception sẽ publish một Message type Fault<AuctionCreated> vào fault queue
			//define một consumer tên AuctionCreatedConsumerFault bên AuctionSvc để pick it up
			if (item.Model == "Foo") throw new ArgumentException("Cannot sell cars with name of Foo");

			await item.SaveAsync();
		}
	}
}