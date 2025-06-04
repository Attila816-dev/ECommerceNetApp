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

        public static LoginUserCommandResponse Successful(string accessToken, string refreshToken)
            => new LoginUserCommandResponse
            {
                Success = true,
                Message = "Login successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };

        public static LoginUserCommandResponse Failed(string message)
            => new LoginUserCommandResponse
            {
                Success = false,
                Message = message,
                AccessToken = null,
                RefreshToken = null,
            };
    }
}
