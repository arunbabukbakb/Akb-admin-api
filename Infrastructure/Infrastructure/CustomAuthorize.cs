using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Infrastructure.Infrastructure
{
    public class CustomAuthorize : Attribute, IAuthorizationFilter
    {
        readonly object[] _roles;

        public CustomAuthorize(params object[] roles)
        {
            _roles = roles;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(a => a.GetType().Name == "AllowAnonymousAttribute")) return;

            var hasClaim = context.HttpContext.User.Claims.Any(c => _roles.Contains(c.Value));
            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
