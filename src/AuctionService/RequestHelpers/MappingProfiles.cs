using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;

namespace AuctionService.RequestHelpers
{
	public class MappingProfiles : Profile //inherit from AutoMapper
	{
		public MappingProfiles()
		{
			//đọc thêm về IncludeMembers tại https://code-maze.com/automapper-net-core-custom-projections/
			CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
			CreateMap<Item, AuctionDto>();

			//đọc thêm về ForMembers và MapFrom tại https://codelearn.io/sharing/su-dung-automapper-trong-csharp
			//ở đây map nguyên cái CreateAuctionDto vào property Item của Auction
			CreateMap<CreateAuctionDto, Auction>().
				ForMember(destination => destination.Item, option => option.MapFrom(source => source));
			CreateMap<CreateAuctionDto, Item>();
		}
	}
}