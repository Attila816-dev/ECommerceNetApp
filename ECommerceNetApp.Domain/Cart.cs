namespace ECommerceNetApp.Domain
{
    public class Cart
    {
        public string? Id { get; set; }

        public List<CartItem> Items { get; } = new List<CartItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
