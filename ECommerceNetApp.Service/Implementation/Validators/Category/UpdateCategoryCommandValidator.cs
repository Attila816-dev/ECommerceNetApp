using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Service.Commands.Category;
using FluentValidation;

namespace ECommerceNetApp.Service.Validators.Category
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(command => command.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(CategoryEntity.MaxCategoryNameLength)
                .WithMessage($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");

            RuleFor(command => command.ParentCategoryId)
                .GreaterThan(0).WithMessage("ParentCategory ID must be a valid positive number.")
                .When(command => command.ParentCategoryId.HasValue);
        }
    }
}
