using System.Text;
using ECommerceNetApp.Domain.Options;
using Microsoft.Extensions.Options;

namespace ECommerceNetApp.Api.Services
{
    public class JwtOptionsValidator : IValidateOptions<JwtOptions>
    {
        public ValidateOptionsResult Validate(string? name, JwtOptions options)
        {
            var errors = new List<string>();

            ArgumentNullException.ThrowIfNull(options, nameof(options));

            // Check if the secret key has sufficient entropy
            if (!string.IsNullOrEmpty(options.SecretKey))
            {
                var keyBytes = Encoding.UTF8.GetBytes(options.SecretKey);

                // Check for minimum key size (256 bits recommended for HMAC-SHA256)
                if (keyBytes.Length < 32)
                {
                    errors.Add("Secret key must be at least 32 bytes (256 bits) for adequate security");
                }

                // Check for adequate complexity
                bool hasUpper = options.SecretKey.Any(char.IsUpper);
                bool hasLower = options.SecretKey.Any(char.IsLower);
                bool hasDigit = options.SecretKey.Any(char.IsDigit);
                bool hasSpecial = options.SecretKey.Any(c => !char.IsLetterOrDigit(c));

                if (!(hasUpper && hasLower && hasDigit && hasSpecial))
                {
                    errors.Add("Secret key should contain uppercase, lowercase, digits, and special characters");
                }
            }

            // Validate issuer and audience as URLs if they look like URLs
            if (options.Issuer?.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true &&
                !Uri.TryCreate(options.Issuer, UriKind.Absolute, out _))
            {
                errors.Add("Issuer must be a valid URL if using http/https format");
            }

            if (options.Audience?.StartsWith("http", StringComparison.OrdinalIgnoreCase) == true &&
                !Uri.TryCreate(options.Audience, UriKind.Absolute, out _))
            {
                errors.Add("Audience must be a valid URL if using http/https format");
            }

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }
}
