using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IValidator<DeleteCategoryCommand> validator)
        : IRequestHandler<DeleteCategoryCommand>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IValidator<DeleteCategoryCommand> _validator = validator;

        public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            await _categoryRepository.DeleteAsync(request.Id, cancellationToken).ConfigureAwait(false);
        }
    }
}
