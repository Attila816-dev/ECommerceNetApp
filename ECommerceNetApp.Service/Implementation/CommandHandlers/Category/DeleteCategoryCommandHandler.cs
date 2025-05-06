using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler(
        IProductCatalogUnitOfWork productCatalogUnitOfWork,
        IValidator<DeleteCategoryCommand> validator)
        : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly IProductCatalogUnitOfWork _productCatalogUnitOfWork = productCatalogUnitOfWork;
        private readonly IValidator<DeleteCategoryCommand> _validator = validator;

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _productCatalogUnitOfWork.CategoryRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
            await _productCatalogUnitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
