namespace ECommerceNetApp.Api.GraphQL.InputTypes
{
    // Input types for product mutation
    public class UpdateProductInput
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; } = "EUR";

        public int Amount { get; set; }
    }
}
