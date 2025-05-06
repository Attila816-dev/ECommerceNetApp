namespace ECommerceNetApp.Service.DTO
{
    public class CartItemDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImageAltText { get; set; }

        public decimal Price { get; set; }

        public string? Currency { get; set; }

        public int Quantity { get; set; }
    }
}
