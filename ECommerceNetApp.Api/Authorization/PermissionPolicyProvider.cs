using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Api.Authorization
{
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private const char Separator = ':';

        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            ArgumentNullException.ThrowIfNull(policyName);
            if (policyName.StartsWith(RequirePermissionAttribute.PermissionPolicyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var parts = policyName.Split(Separator);
                if (parts.Length == 3)
                {
                    var action = parts[1];
                    var resource = parts[2];

                    var policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new PermissionRequirement(action, resource))
                        .Build();

                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }
            }

            return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
