
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Infrastructure
{
    public class CustomAuthorizeHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly IConfiguration iconfiguration;
        public CustomAuthorizeHandler(IConfiguration iconfiguration)
        {
            this.iconfiguration = iconfiguration;
        }
        private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
        public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {

            IdentityModelEventSource.ShowPII = true;
            var jwtToken = context.Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var tokenKey = Encoding.UTF8.GetBytes(iconfiguration["JWT:Key"]);

            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = iconfiguration["JWT:ValidIssuer"],
                ValidAudience = iconfiguration["JWT:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(tokenKey)
            }; 
            validationParameters.ValidateLifetime = true;

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);


           // return principal;

            return defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
