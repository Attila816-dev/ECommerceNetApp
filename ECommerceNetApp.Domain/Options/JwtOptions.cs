using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerceNetApp.Domain.Options
{
    public class JwtOptions
    {
        [Required]
        [MinLength(10)]
        public required string SecretKey { get; set; }

        [Required]
        public required string Issuer { get; set; }

        [Required]
        public required string Audience { get; set; }

        [Range(1, 72)]
        public int ExpirationHours { get; set; } = 24;

        public byte[] GetSecretKeyBytes()
        {
            return Encoding.UTF8.GetBytes(SecretKey);
        }
    }
}
