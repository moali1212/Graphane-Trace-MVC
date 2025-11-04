using Activator.Api.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Activator.Api.Middleware
{
    // For demo purposes only: accept headers X-User-Id and X-User-Role
    public class SimpleAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SimpleAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            if (ctx.Request.Headers.TryGetValue("X-User-Id", out var uid) && Guid.TryParse(uid, out var guid))
            {
                var role = Role.User;
                if (ctx.Request.Headers.TryGetValue("X-User-Role", out var r))
                {
                    Enum.TryParse<Role>(r.ToString(), out role);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, guid.ToString()),
                    new Claim(ClaimTypes.Role, role.ToString())
                };
                var identity = new ClaimsIdentity(claims, "simple");
                ctx.User = new ClaimsPrincipal(identity);
            }

            await _next(ctx);
        }
    }
}