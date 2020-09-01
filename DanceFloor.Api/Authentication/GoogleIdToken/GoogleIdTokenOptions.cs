using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace DanceFloor.Api.Authentication.GoogleIdToken
{
    public class GoogleIdTokenOptions : AuthenticationSchemeOptions
    {
        public GoogleIdTokenOptions()
        {
            ClaimsIssuer = GoogleDefaults.AuthenticationScheme;
        }
    }
}