using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.User
{
    public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenCommandResponse>;

    public class RefreshTokenCommandResponse
    {
        public required bool Success { get; init; }

        public required string Message { get; init; }

        public string? AccessToken { get; init; }

        public string? RefreshToken { get; init; }

        public static RefreshTokenCommandResponse Successful(string accessToken, string refreshToken)
            => new RefreshTokenCommandResponse
            {
                Success = true,
                Message = "Tokens refreshed successfully",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

        public static RefreshTokenCommandResponse Failed(string message)
            => new RefreshTokenCommandResponse
            {
                Success = false,
                Message = message,
                AccessToken = null,
                RefreshToken = null,
            };
    }
}
