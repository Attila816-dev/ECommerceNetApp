﻿using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Enums;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface IRolePermissionService
    {
        IEnumerable<Permission> GetPermissionsForRole(UserRole role);

        bool HasPermission(UserRole role, string action, string resource);
    }
}
