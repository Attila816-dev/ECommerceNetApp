using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.User
{
    public record ValidateTokenCommand(string Token, string? TokenType) : ICommand<ValidateTokenCommandResponse>;

    public class ValidateTokenCommandResponse
    {
        public required bool Success { get; init; }

        public string? Message { get; init; }

        public string? TokenId { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; init; }

        public string? TokenType { get; set; } = "Bearer";

        public static ValidateTokenCommandResponse Successful(string? tokenId, string? tokenType, string? email, string? fullName)
            => new ValidateTokenCommandResponse
            {
                Success = true,
                Message = "Tokens validated successfully",
                TokenId = tokenId,
                TokenType = tokenType,
                Email = email,
                FullName = fullName,
            };

        public static ValidateTokenCommandResponse Failed(string? message)
            => new ValidateTokenCommandResponse
            {
                Success = false,
                Message = message ?? "Token validation failed",
            };
    }
}
