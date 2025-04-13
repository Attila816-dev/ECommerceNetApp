namespace ECommerceNetApp.Service.Commands
{
    public record UpdateCartItemQuantityCommand(string CartId, int ItemId, int Quantity);
}
