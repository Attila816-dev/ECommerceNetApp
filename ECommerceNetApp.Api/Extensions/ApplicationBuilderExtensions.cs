using ECommerceNetApp.Api.Middleware;

namespace ECommerceNetApp.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }

        public static IApplicationBuilder UseIdentityLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdentityLoggingMiddleware>();
        }
    }
}
