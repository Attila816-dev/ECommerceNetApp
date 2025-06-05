using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Domain.Enums;

namespace ECommerceNetApp.Service.Implementation
{
    public class RolePermissionService : IRolePermissionService
    {
        private static readonly List<Permission> FullProductPermissions =
        [
            new Permission(Permissions.Read, Resources.Product),
            new Permission(Permissions.Create, Resources.Product),
            new Permission(Permissions.Update, Resources.Product),
            new Permission(Permissions.Delete, Resources.Product),
        ];

        private static readonly List<Permission> FullCategoryPermissions =
        [
            new Permission(Permissions.Read, Resources.Category),
            new Permission(Permissions.Create, Resources.Category),
            new Permission(Permissions.Update, Resources.Category),
            new Permission(Permissions.Delete, Resources.Category),
        ];

        private static readonly List<Permission> FullCartPermissions =
        [
            new Permission(Permissions.Read, Resources.Cart),
            new Permission(Permissions.Create, Resources.Cart),
            new Permission(Permissions.Update, Resources.Cart),
            new Permission(Permissions.Delete, Resources.Cart),
        ];

        private static readonly List<Permission> FullUserPermissions =
        [
            new Permission(Permissions.Read, Resources.User),
            new Permission(Permissions.Create, Resources.User),
            new Permission(Permissions.Update, Resources.User),
            new Permission(Permissions.Delete, Resources.User),
        ];

        private static readonly Dictionary<UserRole, List<Permission>> RolePermissions = new Dictionary<UserRole, List<Permission>>
        {
            [UserRole.Customer] =
            [

                // Store customer: Read only + Cart access
                new Permission(Permissions.Read, Resources.Product),
                new Permission(Permissions.Read, Resources.Category),
                .. FullCartPermissions
            ],
            [UserRole.ProductManager] =
            [

                // Manager: Full CRUD on catalog, Cart access
                .. FullProductPermissions,
                .. FullCategoryPermissions,
                .. FullCartPermissions,
                new Permission(Permissions.Read, Resources.User),
            ],
            [UserRole.Admin] =
            [

                // Admin: Full access to everything
                .. FullProductPermissions,
                .. FullCategoryPermissions,
                .. FullCartPermissions,
                .. FullUserPermissions,
            ],
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
