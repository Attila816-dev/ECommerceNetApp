﻿namespace ECommerceNetApp.Service.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public decimal Price { get; set; }

        public string? Currency { get; set; }

        public int Amount { get; set; }
    }
}
