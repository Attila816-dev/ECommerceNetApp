using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.GraphQL.Authorization
{
    /// <summary>
    /// Authorization handler for GraphQL permission requirements.
    /// </summary>
    public class GraphQLPermissionRequirementHandler : AuthorizationHandler<GraphQLPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            GraphQLPermissionRequirement requirement)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requirement);
            if (context.User.HasClaim("permission", requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
