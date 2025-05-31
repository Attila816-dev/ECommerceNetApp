using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.Authorization
{
    public class PermissionRequirement(string action, string resource)
        : IAuthorizationRequirement
    {
        public string Action { get; } = action;

        public string Resource { get; } = resource;
    }
}
