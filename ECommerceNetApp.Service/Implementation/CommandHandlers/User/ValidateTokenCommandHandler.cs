using ECommerceNetApp.Domain.Authorization;
using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Commands.User;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.User
{
    public class ValidateTokenCommandHandler(
        ITokenService tokenService)
        : ICommandHandler<ValidateTokenCommand, ValidateTokenCommandResponse>
    {
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

        public Task<ValidateTokenCommandResponse> HandleAsync(ValidateTokenCommand command, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            // Determine token type based on request or try to validate as different types
            TokenValidationResultDto validationResult;

            if (!string.IsNullOrEmpty(command.TokenType))
            {
                // Validate specific token type
                validationResult = command.TokenType.ToUpperInvariant() switch
                {
                    "ACCESS" => _tokenService.ValidateToken(command.Token, TokenType.Access),
                    "REFRESH" => _tokenService.ValidateRefreshToken(command.Token),
                    "ID" => _tokenService.ValidateIdToken(command.Token),
                    _ => new TokenValidationResultDto { IsValid = false, Error = "Invalid token type specified. Possible options: ACCESS, REFRESH or ID." },
                };
            }
            else
            {
                // Try to validate as ID token first, then access token
                validationResult = _tokenService.ValidateIdToken(command.Token);
                if (!validationResult.IsValid)
                {
                    validationResult = tokenService.ValidateToken(command.Token, TokenType.Access);
                }
            }

            if (validationResult.IsValid)
            {
                return Task.FromResult(ValidateTokenCommandResponse.Successful(validationResult.TokenId, validationResult.TokenType?.ToString(), validationResult.Email, validationResult.FullName));
            }
            else
            {
                return Task.FromResult(ValidateTokenCommandResponse.Failed(validationResult.Error));
            }
        }
    }
}
