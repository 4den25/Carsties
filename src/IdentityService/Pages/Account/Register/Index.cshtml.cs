using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityService.Models;
using IdentityService.Pages.Account.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IdentityService.Pages.Register
{
	[SecurityHeaders]
	[AllowAnonymous]
	public class Index : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public Index(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		//bind it to the HTML
		[BindProperty]
		public RegisterViewModel Input { get; set; }

		[BindProperty]
		public bool RegisterSuccess { get; set; }

		//we're gonna return the page so we return IActionResult instead of void
		//return Url was passed from query
		public IActionResult OnGet(string returnUrl)
		{
			Input = new RegisterViewModel
			{
				ReturnUrl = returnUrl
			};

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			//redirect to the homepage if user click cancel btn
			if (Input.Button != "register") return Redirect("~/");

			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = Input.Username,
					Email = Input.Email,
					EmailConfirmed = true //we have no way to confirm the email so just set EmailConfirmed to true
				};

				//create user with the infomation that they filled in the form
				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded)
				{
					await _userManager.AddClaimsAsync(user, new Claim[]
					{
						new Claim(JwtClaimTypes.Name, Input.Fullname)
					});

					RegisterSuccess = true;
				}
			}

			return Page();
		}
	}
}