using HotChocolate.Authorization;
using HotChocolate.Resolvers;

namespace ECommerceNetApp.Api.GraphQL.Authorization
{
    /// <summary>
    /// Custom authorization handler for GraphQL that checks permission claims.
    /// </summary>
    public class GraphQLPermissionAuthorizationHandler : IAuthorizationHandler
    {
        public ValueTask<AuthorizeResult> AuthorizeAsync(
            IMiddlewareContext context,
            AuthorizeDirective directive,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(directive);

            var httpContextAccessor = context.Services.GetService<IHttpContextAccessor>();
            var user = httpContextAccessor?.HttpContext?.User;

            // Ensure the user is authenticated
            if (user?.Identity?.IsAuthenticated != true)
            {
                return ValueTask.FromResult(AuthorizeResult.NotAllowed);
            }

            // Check if the directive specifies a policy
            if (!string.IsNullOrEmpty(directive.Policy))
            {
                // Validate user claims against the policy (e.g., permission claims)
                var hasClaim = user.HasClaim("permission", directive.Policy);
                if (!hasClaim)
                {
                    // User does not satisfy the policy
                    return ValueTask.FromResult(AuthorizeResult.NotAllowed);
                }
            }

            // Request is allowed if policy checks pass
            return ValueTask.FromResult(AuthorizeResult.Allowed);
        }

        public ValueTask<AuthorizeResult> AuthorizeAsync(
            AuthorizationContext context,
            IReadOnlyList<AuthorizeDirective> directives,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(directives);
            var httpContextAccessor = context.Services.GetService<IHttpContextAccessor>();
            var user = httpContextAccessor?.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return ValueTask.FromResult(AuthorizeResult.NotAllowed);
            }

            foreach (var directive in directives)
            {
                if (directive.Policy != null)
                {
                    // Check if user has the required permission
                    var hasPermission = user.HasClaim("permission", directive.Policy);
                    if (!hasPermission)
                    {
                        return ValueTask.FromResult(AuthorizeResult.NotAllowed);
                    }
                }
            }

            return ValueTask.FromResult(AuthorizeResult.Allowed);
        }
    }
}
