using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cw3.Handlers
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {

        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing authorization header.");

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var credentialsBytes = Convert.FromBase64String(authHeader.Parameter);
            var credenstials = Encoding.UTF8.GetString(credentialsBytes).Split(":");


            //sprawdzenie loginu i hasla w bazie

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "loginjakis"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name); // 1 argument dane opisujace uzytkownika, 2 argument rodzaj uwierzytelnienia
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);

        }
    }
}
