namespace ECommerceNetApp.Api.Model
{
    // Request model for token validation
    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;

        public string? TokenType { get; set; } // "access", "refresh", "id"
    }
}
