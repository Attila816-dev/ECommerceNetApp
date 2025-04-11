namespace ECommerceNetApp.Domain
{
    public class Cart
    {
        public string? Id { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<CartItem> Items { get; set; } = new List<CartItem>();
#pragma warning restore CA2227 // Collection properties should be read only

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
