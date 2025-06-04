using Microsoft.AspNetCore.Authorization;

namespace ECommerceNetApp.Api.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        public const string PermissionPolicyPrefix = "Permission:";

        public RequirePermissionAttribute(string action, string resource)
        {
            Action = action;
            Resource = resource;
            Policy = $"{PermissionPolicyPrefix}{action}:{resource}";
        }

        public string Action { get; }

        public string Resource { get; }
    }
}
