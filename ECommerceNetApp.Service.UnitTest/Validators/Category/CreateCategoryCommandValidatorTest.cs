using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Validators.Category;
using FluentValidation.TestHelper;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class CreateCategoryCommandValidatorTest
    {
        private readonly CreateCategoryCommandValidator _validator;
        private readonly Mock<ICategoryRepository> _categoryRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _productCatalogUnitOfWork;

        public CreateCategoryCommandValidatorTest()
        {
            _categoryRepository = new Mock<ICategoryRepository>();
            _productCatalogUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _productCatalogUnitOfWork.SetupGet(x => x.CategoryRepository).Returns(_categoryRepository.Object);
            _validator = new CreateCategoryCommandValidator(_productCatalogUnitOfWork.Object);
        }

        [Fact]
        public void Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand(string.Empty, null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public void Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var categoryName = new string('A', CategoryEntity.MaxCategoryNameLength + 1);
            var command = new CreateCategoryCommand(categoryName, null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Category name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
        }

        [Fact]
        public async Task Validate_WithInvalidParentCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, 0);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.Is<int>(x => x != 0), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.Is<int>(x => x == 0), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ParentCategoryId)
                .WithErrorMessage("Parent Category does not exist.");
        }

        [Fact]
        public void Validate_WithNullParentCategoryId_ShouldPassValidation()
        {
            // Arrange
            var command = new CreateCategoryCommand("Valid Category Name", null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
