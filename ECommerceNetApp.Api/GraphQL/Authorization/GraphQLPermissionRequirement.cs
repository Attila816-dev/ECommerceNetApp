using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.GraphQL.Authorization
{
    /// <summary>
    /// Authorization requirement for GraphQL operations.
    /// </summary>
    public class GraphQLPermissionRequirement : IAuthorizationRequirement
    {
        public GraphQLPermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
