using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Product;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Product
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var exists = await _productRepository.ExistsAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (!exists)
            {
                throw new InvalidOperationException($"Product with id {request.Id} not found");
            }

            await _productRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
        }
    }
}
