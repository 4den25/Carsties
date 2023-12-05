using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
	public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
	{
		public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
		{
			Console.WriteLine("--> Consuming fault creation");

			var exception = context.Message.Exceptions.First();

			if (exception.ExceptionType == "System.ArgumentException")
			{
				//message 1 là Fault, message 2 là AuctionCreated
				context.Message.Message.Model = "FooBar";
				//sau khi đổi tên model của msg, 
				//publish lại msg type AuctionCreated cho Consumer bên SearchSvc pick up
				await context.Publish(context.Message.Message);
			}
			else
			{
				Console.WriteLine("Not an argument exception - update error dashboard somewhere");
			}
		}
	}
}