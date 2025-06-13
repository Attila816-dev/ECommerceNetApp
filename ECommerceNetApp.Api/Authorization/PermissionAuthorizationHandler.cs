using System.Security.Claims;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.Authorization
{
    public class PermissionAuthorizationHandler(
        IRolePermissionService rolePermissionService,
        ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<PermissionRequirement>
    {
        private static readonly Action<ILogger, Exception?> LogUserRoleNotFound =
            LoggerMessage.Define(LogLevel.Warning, new EventId(0, nameof(PermissionAuthorizationHandler)), "User role not found in claims");

        private static readonly Action<ILogger, UserRole?, string, string, Exception?> LogPermissionGranted =
            LoggerMessage.Define<UserRole?, string, string>(
                LogLevel.Information,
                new EventId(1, nameof(PermissionAuthorizationHandler)),
                "Permission granted: {Role} can {Action} {Resource}");

        private static readonly Action<ILogger, UserRole, string, string, Exception?> LogPermissionDenied =
            LoggerMessage.Define<UserRole, string, string>(
                LogLevel.Warning,
                new EventId(1, nameof(PermissionAuthorizationHandler)),
                "Permission denied: {Role} cannot {Action} {Resource}");

        private readonly IRolePermissionService _rolePermissionService = rolePermissionService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger = logger;

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requirement);

            // Check if user has the specific permission claim (added during JWT validation)
            var permissionClaim = $"{requirement.Action}:{requirement.Resource}";
            if (context.User.HasClaim("permission", permissionClaim))
            {
                LogPermissionGranted(_logger, GetUserRole(context.User), requirement.Action, requirement.Resource, null);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Fallback: Check role-based permissions if permission claims are not present
            var userRole = GetUserRole(context.User);
            if (userRole == null)
            {
                LogUserRoleNotFound(_logger, null);
                context.Fail();
                return Task.CompletedTask;
            }

            if (_rolePermissionService.HasPermission(userRole.Value, requirement.Action, requirement.Resource))
            {
                LogPermissionGranted(_logger, userRole.Value, requirement.Action, requirement.Resource, null);
                context.Succeed(requirement);
            }
            else
            {
                LogPermissionDenied(_logger, userRole.Value, requirement.Action, requirement.Resource, null);
                context.Fail();
            }

            return Task.CompletedTask;
        }

        private static UserRole? GetUserRole(ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

            return roleClaim != null && Enum.TryParse<UserRole>(roleClaim, out var role)
                ? role
                : null;
        }
    }
}
