namespace ECommerceNetApp.Api.Model
{
    // Request model for token validation
    public class ValidateTokenRequest
    {
        public required string Token { get; set; }

        public string? TokenType { get; set; } // "access", "refresh", "id"
    }
}
