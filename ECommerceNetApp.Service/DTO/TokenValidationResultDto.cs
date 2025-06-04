namespace ECommerceNetApp.Service.DTO
{
    public class TokenValidationResultDto
    {
        public bool IsValid { get; set; }

        public string? Error { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }

        public string? FullName { get; set; }
    }
}
