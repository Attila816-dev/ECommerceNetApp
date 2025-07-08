using Azure.Core;
using ECommerceNetApp.Grpc.Protos;
using ECommerceNetApp.Service.Commands.Cart;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Cart;
using Grpc.Core;

namespace ECommerceNetApp.Grpc.Services
{
#pragma warning disable CA1848 // Use the LoggerMessage delegates
    public class CartGrpcService : CartService.CartServiceBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly ILogger<CartGrpcService> _logger;

        public CartGrpcService(IDispatcher dispatcher, ILogger<CartGrpcService> logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 1. Get list of items of the cart object – unary rpc call
        public override async Task<GetCartItemsResponse> GetCartItems(
            GetCartItemsRequest request,
            ServerCallContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(request);
            _logger.LogInformation("GetCartItems called for CartId: {CartId}", request.CartId);

            try
            {
                var query = new GetCartItemsQuery(request.CartId);
                var cartItems = await _dispatcher.SendQueryAsync<GetCartItemsQuery, List<CartItemDto>?>(
                    query, context.CancellationToken).ConfigureAwait(false);

                if (cartItems == null)
                {
                    _logger.LogWarning("Cart not found for CartId: {CartId}", request.CartId);
                    throw new RpcException(new Status(StatusCode.NotFound, "Cart not found"));
                }

                // Get cart total
                var totalQuery = new GetCartTotalQuery(request.CartId);
                var total = await _dispatcher.SendQueryAsync<GetCartTotalQuery, decimal?>(
                    totalQuery, context.CancellationToken).ConfigureAwait(false);

                var response = new GetCartItemsResponse
                {
                    CartId = request.CartId,
                    TotalAmount = (double)(total ?? 0),
                };

                response.Items.AddRange(cartItems.Select(MapToGrpcCartItem));

                _logger.LogInformation("GetCartItems completed for CartId: {CartId}, ItemCount: {ItemCount}", request.CartId, cartItems.Count);

                return response;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetCartItems operation cancelled for CartId: {CartId}", request.CartId);
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation was cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCartItems for CartId: {CartId}", request.CartId);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        // 2. Get list of items of the cart object – server stream rpc call
        public override async Task GetCartItemsStream(
            GetCartItemsRequest request,
            IServerStreamWriter<CartItemResponse> responseStream,
            ServerCallContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(responseStream);
            _logger.LogInformation("GetCartItemsStream called for CartId: {CartId}", request.CartId);

            try
            {
                var query = new GetCartItemsQuery(request.CartId);
                var cartItems = await _dispatcher.SendQueryAsync<GetCartItemsQuery, List<CartItemDto>?>(
                    query, context.CancellationToken).ConfigureAwait(false);

                if (cartItems == null)
                {
                    _logger.LogWarning("Cart not found for CartId: {CartId}", request.CartId);
                    throw new RpcException(new Status(StatusCode.NotFound, "Cart not found"));
                }

                _logger.LogInformation("Starting to stream {ItemCount} items for CartId: {CartId}", cartItems.Count, request.CartId);

                for (int i = 0; i < cartItems.Count; i++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var itemResponse = new CartItemResponse
                    {
                        Item = MapToGrpcCartItem(cartItems[i]),
                        IsLast = i == cartItems.Count - 1,
                    };

                    await responseStream.WriteAsync(itemResponse).ConfigureAwait(false);

                    _logger.LogDebug("Streamed item {ItemIndex}/{TotalItems} for CartId: {CartId}", i + 1, cartItems.Count, request.CartId);

                    // Small delay to simulate real streaming behavior
                    await Task.Delay(50, context.CancellationToken).ConfigureAwait(false);
                }

                _logger.LogInformation("GetCartItemsStream completed for CartId: {CartId}", request.CartId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("GetCartItemsStream operation cancelled for CartId: {CartId}", request.CartId);
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation was cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCartItemsStream for CartId: {CartId}", request.CartId);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        // 3. Add item to the cart and return updated cart object – client stream rpc call
        public override async Task<GetCartItemsResponse> AddItemsToCart(
            IAsyncStreamReader<AddCartItemRequest> requestStream,
            ServerCallContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requestStream);
            _logger.LogInformation("AddItemsToCart client stream started");

            try
            {
                string? cartId = null;
                var itemsAdded = 0;

                await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken).ConfigureAwait(false))
                {
                    cartId ??= request.CartId;

                    if (cartId != request.CartId)
                    {
                        _logger.LogWarning("Inconsistent CartId in stream: expected {ExpectedCartId}, got {ActualCartId}", cartId, request.CartId);
                        throw new RpcException(new Status(StatusCode.InvalidArgument, "All requests must have the same CartId"));
                    }

                    var cartItemDto = MapToCartItemDto(request.Item);
                    var command = new AddCartItemCommand(request.CartId, cartItemDto);

                    await _dispatcher.SendCommandAsync(command, context.CancellationToken).ConfigureAwait(false);
                    itemsAdded++;

                    _logger.LogDebug("Added item {ItemId} to cart {CartId}", request.Item.Id, request.CartId);
                }

                if (cartId == null)
                {
                    _logger.LogWarning("No items received in client stream");
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "No items provided"));
                }

                _logger.LogInformation("AddItemsToCart completed for CartId: {CartId}, ItemsAdded: {ItemsAdded}", cartId, itemsAdded);

                // Return updated cart
                var getCartRequest = new GetCartItemsRequest { CartId = cartId };
                return await GetCartItems(getCartRequest, context).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AddItemsToCart operation cancelled");
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation was cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddItemsToCart");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        // 4. Add item to the cart and return updated cart object – bi-directional rpc call
        public override async Task AddItemsToCartBidirectional(
            IAsyncStreamReader<AddCartItemRequest> requestStream,
            IServerStreamWriter<GetCartItemsResponse> responseStream,
            ServerCallContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requestStream);
            ArgumentNullException.ThrowIfNull(responseStream);
            _logger.LogInformation("AddItemsToCartBidirectional started");

            try
            {
                await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken).ConfigureAwait(false))
                {
                    _logger.LogDebug("Processing bidirectional request for CartId: {CartId}, ItemId: {ItemId}", request.CartId, request.Item.Id);

                    var cartItemDto = MapToCartItemDto(request.Item);
                    var command = new AddCartItemCommand(request.CartId, cartItemDto);

                    await _dispatcher.SendCommandAsync(command, context.CancellationToken).ConfigureAwait(false);

                    // Get updated cart and stream it back
                    var getCartRequest = new GetCartItemsRequest { CartId = request.CartId };
                    var updatedCart = await GetCartItems(getCartRequest, context).ConfigureAwait(false);

                    await responseStream.WriteAsync(updatedCart).ConfigureAwait(false);

                    _logger.LogDebug("Sent updated cart for CartId: {CartId} with {ItemCount} items", request.CartId, updatedCart.Items.Count);
                }

                _logger.LogInformation("AddItemsToCartBidirectional completed");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("AddItemsToCartBidirectional operation cancelled");
                throw new RpcException(new Status(StatusCode.Cancelled, "Operation was cancelled"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddItemsToCartBidirectional");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        private static CartItem MapToGrpcCartItem(CartItemDto dto)
        {
            return new CartItem
            {
                Id = dto.Id,
                Name = dto.Name,
                Price = (double)dto.Price,
                Currency = dto.Currency,
                Quantity = dto.Quantity,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                ImageAltText = dto.ImageAltText ?? string.Empty,
                TotalPrice = (double)dto.Price * dto.Quantity,
            };
        }

        private static CartItemDto MapToCartItemDto(CartItem grpcItem)
        {
            return new CartItemDto
            {
                Id = grpcItem.Id,
                Name = grpcItem.Name,
                Price = (decimal)grpcItem.Price,
                Currency = grpcItem.Currency,
                Quantity = grpcItem.Quantity,
                ImageUrl = string.IsNullOrEmpty(grpcItem.ImageUrl) ? null : grpcItem.ImageUrl,
                ImageAltText = string.IsNullOrEmpty(grpcItem.ImageAltText) ? null : grpcItem.ImageAltText,
            };
        }
    }
#pragma warning restore CA1848 // Use the LoggerMessage delegates
}
