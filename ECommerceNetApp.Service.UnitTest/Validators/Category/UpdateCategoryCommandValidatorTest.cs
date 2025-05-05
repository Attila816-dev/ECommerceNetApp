using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Validators.Category;
using FluentValidation.TestHelper;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class UpdateCategoryCommandValidatorTest
    {
        private readonly UpdateCategoryCommandValidator _validator;
        private readonly Mock<ICategoryRepository> _categoryRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;

        public UpdateCategoryCommandValidatorTest()
        {
            _categoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CategoryRepository).Returns(_categoryRepository.Object);
            _validator = new UpdateCategoryCommandValidator(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Validate_WithValidData_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyName_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, string.Empty, null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage("Category name is required.");
        }

        [Fact]
        public async Task Validate_WithNameExceedingMaxLength_ShouldFailValidation()
        {
            // Arrange
            var categoryName = new string('A', CategoryEntity.MaxCategoryNameLength + 1);
            var command = new UpdateCategoryCommand(1, categoryName, null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name)
                .WithErrorMessage($"Category Name cannot exceed {CategoryEntity.MaxCategoryNameLength} characters.");
        }

        [Fact]
        public async Task Validate_WithInvalidParentCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, 0);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.Is<int>(x => x == 1), It.IsAny<CancellationToken>()))
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
        public async Task Validate_WithInvalidCategoryId_ShouldFailValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.Is<int>(x => x == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Category does not exist.");
        }

        [Fact]
        public async Task Validate_WithNullParentCategoryId_ShouldPassValidation()
        {
            // Arrange
            var command = new UpdateCategoryCommand(1, "Valid Category Name", null, null);

            _categoryRepository.Setup(repo => repo.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.ParentCategoryId);
        }
    }
}
