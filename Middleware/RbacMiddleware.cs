namespace RBFSS.Middleware
{
    /// <summary>
    /// RBAC Middleware — enforces role-based access at the HTTP pipeline level,
    /// before requests reach controllers. Provides a consistent 403 response and
    /// logs all denied access attempts.
    /// </summary>
    public class RbacMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RbacMiddleware> _logger;

        // Maps route prefixes to the roles allowed to access them
        private static readonly List<(string RoutePrefix, string[] AllowedRoles)> _routeRules = new()
        {
            ("/admin",          new[] { "Admin" }),
            ("/folder/create",  new[] { "Admin", "Manager" }),
            ("/folder/delete",  new[] { "Admin", "Manager" }),
            ("/manager",        new[] { "Admin", "Manager" }),
        };

        public RbacMiddleware(RequestDelegate next, ILogger<RbacMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLower() ?? "";
                var method = context.Request.Method;
                var userName = context.User.Identity.Name ?? "Unknown";

                foreach (var (routePrefix, allowedRoles) in _routeRules)
                {
                    if (path.StartsWith(routePrefix))
                    {
                        var hasPermission = allowedRoles.Any(role => context.User.IsInRole(role));

                        if (!hasPermission)
                        {
                            _logger.LogWarning(
                                "RBAC DENIED: User '{User}' attempted {Method} {Path}. " +
                                "Required roles: [{Roles}]",
                                userName, method, path, string.Join(", ", allowedRoles));

                            context.Response.Redirect("/Account/AccessDenied");
                            return;
                        }

                        _logger.LogInformation(
                            "RBAC ALLOWED: User '{User}' accessed {Method} {Path}",
                            userName, method, path);

                        break;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RbacMiddlewareExtensions
    {
        public static IApplicationBuilder UseRbac(this IApplicationBuilder builder)
            => builder.UseMiddleware<RbacMiddleware>();
    }
}
