using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Implementation
{
    public class RolePermissionService : IRolePermissionService
    {
        private static readonly Dictionary<UserRole, List<Permission>> RolePermissions = new Dictionary<UserRole, List<Permission>>
        {
            [UserRole.Customer] = new List<Permission>
            {
                // Store customer: Read only + Cart access
                new Permission(Permissions.Read, Resources.Product),
                new Permission(Permissions.Read, Resources.Category),
                new Permission(Permissions.Read, Resources.Cart),
                new Permission(Permissions.Create, Resources.Cart),
                new Permission(Permissions.Update, Resources.Cart),
                new Permission(Permissions.Delete, Resources.Cart),
            },
            [UserRole.ProductManager] = new List<Permission>
            {
                // Manager: Full CRUD on catalog, Cart access
                new Permission(Permissions.Read, Resources.Product),
                new Permission(Permissions.Create, Resources.Product),
                new Permission(Permissions.Update, Resources.Product),
                new Permission(Permissions.Delete, Resources.Product),
                new Permission(Permissions.Read, Resources.Category),
                new Permission(Permissions.Create, Resources.Category),
                new Permission(Permissions.Update, Resources.Category),
                new Permission(Permissions.Delete, Resources.Category),
                new Permission(Permissions.Read, Resources.Cart),
                new Permission(Permissions.Create, Resources.Cart),
                new Permission(Permissions.Update, Resources.Cart),
                new Permission(Permissions.Delete, Resources.Cart),
            },
            [UserRole.Admin] = new List<Permission>
            {
                // Admin: Full access to everything
                new Permission(Permissions.Read, Resources.Product),
                new Permission(Permissions.Create, Resources.Product),
                new Permission(Permissions.Update, Resources.Product),
                new Permission(Permissions.Delete, Resources.Product),
                new Permission(Permissions.Read, Resources.Category),
                new Permission(Permissions.Create, Resources.Category),
                new Permission(Permissions.Update, Resources.Category),
                new Permission(Permissions.Delete, Resources.Category),
                new Permission(Permissions.Read, Resources.Cart),
                new Permission(Permissions.Create, Resources.Cart),
                new Permission(Permissions.Update, Resources.Cart),
                new Permission(Permissions.Delete, Resources.Cart),
                new Permission(Permissions.Read, Resources.User),
                new Permission(Permissions.Create, Resources.User),
                new Permission(Permissions.Update, Resources.User),
                new Permission(Permissions.Delete, Resources.User),
            },
        };

        public IEnumerable<Permission> GetPermissionsForRole(UserRole role)
        {
            return RolePermissions.TryGetValue(role, out var permissions)
                ? permissions
                : Enumerable.Empty<Permission>();
        }

        public bool HasPermission(UserRole role, string action, string resource)
        {
            var permissions = GetPermissionsForRole(role);
            return permissions.Any(p => p.Action == action && p.Resource == resource);
        }
    }
}
