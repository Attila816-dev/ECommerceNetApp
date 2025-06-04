using System.Security.Claims;
using ECommerceNetApp.Domain.Enums;
using ECommerceNetApp.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerceNetApp.Service.Implementation
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public UserRole? UserRole
        {
            get
            {
                var roleValue = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
                return roleValue != null && Enum.TryParse<UserRole>(roleValue, out var role) ? role : null;
            }
        }

        public bool IsInRole(UserRole role)
        {
            return UserRole == role;
        }
    }
}
