﻿using ECommerceNetApp.Domain.Interfaces;
using ECommerceNetApp.Service.Implementation;
using ECommerceNetApp.Service.Implementation.Mappers.Cart;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
using ECommerceNetApp.Service.Implementation.Mappers.Product;
using ECommerceNetApp.Service.Interfaces.Mappers.Cart;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Interfaces.Mappers.Product;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceNetApp.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddECommerceServices(this IServiceCollection services)
        {
            services.AddScoped<ICartItemMapper, CartItemMapper>();
            services.AddScoped<ICategoryMapper, CategoryMapper>();
            services.AddScoped<IProductMapper, ProductMapper>();
            services.AddScoped<IDomainEventService, DomainEventService>();
            return services;
        }
    }
}
