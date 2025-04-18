﻿using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;

namespace ECommerceNetApp.Service.Interfaces
{
    public interface ICartItemMapper
    {
        CartItemDto MapToDto(CartItem item);

        CartItem MapToEntity(AddCartItemCommand command);
    }
}