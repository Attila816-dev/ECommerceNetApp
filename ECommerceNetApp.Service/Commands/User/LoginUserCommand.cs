using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.User
{
    public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserCommandResponse>;

    public class LoginUserCommandResponse
    {
        public required bool Success { get; init; }

        public required string Message { get; init; }

        public string? AccessToken { get; init; }

        public string? RefreshToken { get; init; }

        public string? IdToken { get; init; }

        public DateTime? TokenExpiration { get; init; }

        public static LoginUserCommandResponse Successful(string accessToken, string refreshToken, string idToken, int expirationHours)
        {
            return new LoginUserCommandResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdToken = idToken,
                TokenExpiration = DateTime.UtcNow.AddHours(expirationHours),
            };
        }

        public static LoginUserCommandResponse Failed(string message)
        {
            return new LoginUserCommandResponse
            {
                Success = false,
                Message = message,
            };
        }
    }
}
