using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.CommandHandlers.Category
{
    public class CreateCategoryCommandHandler(
            ICategoryRepository categoryRepository,
            IValidator<CreateCategoryCommand> validator) : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IValidator<CreateCategoryCommand> _validator = validator;

        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            CategoryEntity? parentCategory = null;
            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken).ConfigureAwait(false);
                if (parentCategory == null)
                {
                    throw new InvalidOperationException($"Parent category with id {request.ParentCategoryId.Value} not found");
                }
            }

            var category = new CategoryEntity(request.Name, request.ImageUrl, parentCategory);
            await _categoryRepository.AddAsync(category, cancellationToken).ConfigureAwait(false);

            return category.Id;
        }
    }
}
