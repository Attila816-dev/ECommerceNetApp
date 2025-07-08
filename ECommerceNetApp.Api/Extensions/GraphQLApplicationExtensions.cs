using HotChocolate.AspNetCore;

namespace ECommerceNetApp.Api.Extensions
{
    /// <summary>
    /// Extension methods for configuring GraphQL in the application pipeline.
    /// </summary>
    public static class GraphQLApplicationExtensions
    {
        /// <summary>
        /// Configures GraphQL middleware in the application pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder for chaining.</returns>
        public static IApplicationBuilder UseGraphQL(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL("/graphql")
                    .WithOptions(new GraphQLServerOptions
                    {
                        Tool = { Enable = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(), },
                    });
            });

            return app;
        }
    }
}