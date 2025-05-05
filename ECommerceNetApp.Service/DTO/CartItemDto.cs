using ECommerceNetApp.Domain.ValueObjects;

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

        public static CartItemDto Create(CartItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new CartItemDto
            {
                Id = item.Id,
                Name = item.Name,
                ImageUrl = item.Image?.Url,
                ImageAltText = item.Image?.AltText,
                Price = item.Price?.Amount ?? 0,
                Currency = item.Price?.Currency,
                Quantity = item.Quantity,
            };
        }
    }
}
