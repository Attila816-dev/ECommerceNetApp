﻿using ECommerceNetApp.Domain.Interfaces;

namespace ECommerceNetApp.Service.Commands.Cart
{
    public record UpdateCartItemQuantityCommand(string CartId, int ItemId, int Quantity) : ICommand;
}
