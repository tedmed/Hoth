using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Http.Resilience;

namespace WeatherEye.Web
{
    internal static class LoginLogoutEndpointRouteBuilderExtensions
    {
        internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("authentication");


            group.MapGet(pattern: "/login", OnLogin).AllowAnonymous();
            group.MapGet(pattern: "/logout", OnLogout);

            return group;
        }

        static ChallengeHttpResult OnLogin() =>
            TypedResults.Challenge(properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = "/signin-oidc"
            });
        static SignOutHttpResult OnLogout() =>
            TypedResults.SignOut(properties: new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                RedirectUri = "/signout-callback-oidc"
            },
                [
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme
                ]
            );

    }
}
