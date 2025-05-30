using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.User
{
    public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserCommandResponse>;

    public class LoginUserCommandResponse
    {
        public required bool Success { get; init; }

        public required string Message { get; init; }

        public string? Token { get; init; }

        public static LoginUserCommandResponse Successful(string token)
            => new LoginUserCommandResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
            };

        public static LoginUserCommandResponse Failed(string message)
            => new LoginUserCommandResponse
            {
                Success = false,
                Message = message,
                Token = null,
            };
    }
}
