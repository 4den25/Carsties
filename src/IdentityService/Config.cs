using Duende.IdentityServer.Models;

namespace IdentityService;

public static class Config
{
	public static IEnumerable<IdentityResource> IdentityResources =>
		new IdentityResource[]
		{
			new IdentityResources.OpenId(),
			new IdentityResources.Profile(),
		};

	public static IEnumerable<ApiScope> ApiScopes =>
		new ApiScope[]
		{
			new ApiScope("auctionApp", "Auction App full access")
		};

	public static IEnumerable<Client> Clients =>
		new Client[]
		{
			//define client trong này, postman gửi request tới với param giống với trong này define
			//thì return token, không thì return lỗi invalid_client
			new Client
			{
				ClientId = "postman",
				ClientName = "postman",
				//we allow client to request OpenID, Profile and the scope
				//cuz we're using OpenID, there're going to be 2 tokens returned from this request: ID token and access token
				//access token is the key that allows client request resource from our resource server
				//ID token contains information about user
				AllowedScopes = {"openid", "profile", "auctionApp"}, 
				//we use postman so we won't redirect to any where but just define it
				RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
				ClientSecrets = new[] {new Secret("NotASecret".Sha256())},
				//the password is the authentication flow that allow us to request the token from identity server
				AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
			}
		};
}
