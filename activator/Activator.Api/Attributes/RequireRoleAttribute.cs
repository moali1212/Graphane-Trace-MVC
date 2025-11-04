using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Activator.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRoleAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _role;
        public RequireRoleAttribute(string role) { _role = role; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            if (!roles.Contains(_role))
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}