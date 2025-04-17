﻿namespace ECommerceNetApp.Service
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public decimal Price { get; set; }

        public int Amount { get; set; }
    }
}
