using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace DanceFloor.Api.Extensions
{
    public static class AuthExtensions
    {
        private const string LoginProviderKey = "LoginProvider";
        private const string XsrfKey = "XsrfId";
        
        public static async Task<ExternalLoginInfo> GetExternalLoginInfoAsync<TUser>(this SignInManager<TUser> signInManager, string scheme, string expectedXsrf = null)
            where TUser : class
        {
            // var scheme = signInManager.Context.Request.Headers["Scheme"];
            var auth = await signInManager.Context.AuthenticateAsync(scheme);
            var items = auth?.Properties?.Items;
            if (auth?.Principal == null || items == null || !items.ContainsKey(LoginProviderKey))
            {
                return null;
            }

            if (expectedXsrf != null)
            {
                if (!items.ContainsKey(XsrfKey))
                {
                    return null;
                }
                var userId = items[XsrfKey] as string;
                if (userId != expectedXsrf)
                {
                    return null;
                }
            }
            
            var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var provider = items[LoginProviderKey];
            if (providerKey == null || provider == null)
            {
                return null;
            }

            var providerDisplayName = (await signInManager.GetExternalAuthenticationSchemesAsync())
                .FirstOrDefault(p => p.Name == provider)?.DisplayName ?? provider;
            return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
            {
                AuthenticationTokens = auth.Properties.GetTokens(),
                AuthenticationProperties = auth.Properties
            };
        }
    }
}