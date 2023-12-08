using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
	public class CustomProfileService : IProfileService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public CustomProfileService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		//request and add additional information to our token
		public async Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			var user = await _userManager.GetUserAsync(context.Subject); //subject in token is the user id
			var existingClaims = await _userManager.GetClaimsAsync(user); //the only claim we add is the fullname and we stored that in name claim (Register/Index.cshtml.cs)

			//pass those claims back within the token
			//cuz we gonna need the username for auction service
			var claims = new List<Claim>
			{
				new Claim("username", user.UserName)
			};

			//add claim vừa tạo vào list claim của user
			context.IssuedClaims.AddRange(claims);
			//add claim fullname vào list claim của user
			//we pass the username và fullname within token by this way
			context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));
		}

		public Task IsActiveAsync(IsActiveContext context)
		{
			//we dont need anything with this method
			return Task.CompletedTask;
		}
	}
}