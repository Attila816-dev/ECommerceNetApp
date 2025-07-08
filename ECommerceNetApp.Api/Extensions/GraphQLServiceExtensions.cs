using ECommerceNetApp.Api.GraphQL.Authorization;
using ECommerceNetApp.Api.GraphQL.InputTypes;
using ECommerceNetApp.Api.GraphQL.Resolvers;
using ECommerceNetApp.Api.GraphQL.Types;
using HotChocolate.Authorization;

namespace ECommerceNetApp.Api.Extensions
{
    /// <summary>
    /// Extension methods for configuring GraphQL services.
    /// </summary>
    public static class GraphQLServiceExtensions
    {
        /// <summary>
        /// Adds GraphQL services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
        {
            services
                .AddGraphQLServer()
                .AddQueryType(q => q.Name("Query"))
                .AddMutationType(m => m.Name("Mutation"))
                .AddTypeExtension<CatalogQueryResolvers>()
                .AddTypeExtension<CatalogMutationResolvers>()
                .AddType<CategoryType>()
                .AddType<CategoryDetailType>()
                .AddType<ProductType>()
                .AddType<PaginatedProductsType>()
                .AddType<CreateCategoryInputType>()
                .AddType<UpdateCategoryInputType>()
                .AddType<CreateProductInputType>()
                .AddType<UpdateProductInputType>()
                .AddAuthorization()
                .AddProjections()
                .AddFiltering()
                .AddSorting()
                .ModifyCostOptions(opt =>
                {
                    opt.MaxTypeCost = 2000; // Set a maximum cost for queries
                    opt.MaxFieldCost = 1000; // Set a maximum cost for fields
                })
                .ModifyRequestOptions(opt =>
                {
                    opt.IncludeExceptionDetails = true; // Remove in production
                })
                .ModifyOptions(opt =>
                {
                    opt.DefaultBindingBehavior = BindingBehavior.Explicit;
                    opt.UseXmlDocumentation = true;
                });

            // Add GraphQL authorization policies
            services.AddAuthorization(options =>
            {
                // Category permissions
                options.AddPolicy("Create:Category", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Create:Category")));

                options.AddPolicy("Update:Category", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Update:Category")));

                options.AddPolicy("Delete:Category", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Delete:Category")));

                // Product permissions
                options.AddPolicy("Create:Product", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Create:Product")));

                options.AddPolicy("Update:Product", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Update:Product")));

                options.AddPolicy("Delete:Product", policy =>
                    policy.Requirements.Add(new GraphQLPermissionRequirement("Delete:Product")));
            });

            // Register GraphQL authorization handlers
            services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, GraphQLPermissionRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, GraphQLPermissionAuthorizationHandler>();
            return services;
        }
    }
}