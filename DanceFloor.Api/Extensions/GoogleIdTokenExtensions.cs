using System;
using DanceFloor.Api.Authentication.GoogleIdToken;
using Microsoft.AspNetCore.Authentication;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleIdTokenExtensions
    {
        public static AuthenticationBuilder AddGoogleIdToken(this AuthenticationBuilder builder)
            => builder.AddGoogleIdToken(GoogleIdTokenDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddGoogleIdToken(this AuthenticationBuilder builder, Action<GoogleIdTokenOptions> configureOptions)
            => builder.AddGoogleIdToken(GoogleIdTokenDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddGoogleIdToken(this AuthenticationBuilder builder, string authenticationScheme, Action<GoogleIdTokenOptions> configureOptions)
            => builder.AddGoogleIdToken(authenticationScheme, GoogleIdTokenDefaults.DisplayName, configureOptions);
        
        public static AuthenticationBuilder AddGoogleIdToken(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<GoogleIdTokenOptions> configureOptions)
            => builder.AddScheme<GoogleIdTokenOptions, GoogleIdTokenHandler>(authenticationScheme, displayName, configureOptions);
    }
}