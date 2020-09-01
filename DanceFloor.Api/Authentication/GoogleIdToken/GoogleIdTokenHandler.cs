using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DanceFloor.Api.Authentication.GoogleIdToken
{
    public class GoogleIdTokenHandler : AuthenticationHandler<GoogleIdTokenOptions>
    {
        public GoogleIdTokenHandler(
            IOptionsMonitor<GoogleIdTokenOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                var token = Request.Headers["IdToken"];
                
                if (string.IsNullOrEmpty(token))
                    return AuthenticateResult.NoResult();
                        
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                
                if (payload == null)
                    return AuthenticateResult.Fail("Token is not valid!");
                
                var authTokens = new List<AuthenticationToken>();
                
                if (payload.ExpirationTimeSeconds.HasValue)
                {
                    var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(payload.ExpirationTimeSeconds.Value);
                
                    authTokens.Add(new AuthenticationToken
                    {
                        Name = "expires_at",
                        Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                    });
                }

                var claims = new []
                {
                    new Claim(ClaimTypes.NameIdentifier, payload.Subject),
                    new Claim(ClaimTypes.Email, payload.Email),
                    new Claim(ClaimTypes.Name, payload.Name),
                    new Claim(ClaimTypes.GivenName, payload.GivenName),
                    new Claim(ClaimTypes.Surname, payload.FamilyName), 
                    new Claim(ClaimTypes.Locality, payload.Locale), 
                };
                
                var identity = new ClaimsIdentity(claims, ClaimsIssuer);
                var principal = new ClaimsPrincipal(identity);
                var properties = new AuthenticationProperties
                {
                    Items =
                    {
                        ["LoginProvider"] = GoogleDefaults.DisplayName
                    },
                    Parameters =
                    {
                        ["EmailVerified"] = payload.EmailVerified,
                        ["Expiration"] = payload.ExpirationTimeSeconds.HasValue ? (int)Math.Min(payload.ExpirationTimeSeconds.Value, int.MaxValue) : (int?)null,
                        ["IssuedAt"] = payload.IssuedAtTimeSeconds.HasValue ? (int)Math.Min(payload.IssuedAtTimeSeconds.Value, int.MaxValue) : (int?)null,
                        ["NotBefore"] = payload.NotBeforeTimeSeconds.HasValue ? (int)Math.Min(payload.NotBeforeTimeSeconds.Value, int.MaxValue) : (int?)null
                    }
                };
                
                properties.StoreTokens(authTokens);

                var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);
                
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }
    }
}