using ECommerceNetApp.Domain.Authorization;

namespace ECommerceNetApp.Service.DTO
{
    public class TokenValidationResultDto
    {
        public bool IsValid { get; set; }

        public string? Error { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public string? FullName { get; set; }

        /// <summary>
        /// Gets or sets JWT ID (jti) claim - unique identifier for token invalidation tracking.
        /// </summary>
        public string? TokenId { get; set; }

        public TokenType? TokenType { get; set; }
    }
}
