using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Validators.Category;
using FluentValidation.TestHelper;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.Validators.Category
{
    public class DeleteCategoryCommandValidatorTest
    {
        private readonly DeleteCategoryCommandValidator _validator;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;

        public DeleteCategoryCommandValidatorTest()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _validator = new DeleteCategoryCommandValidator(_mockCategoryRepository.Object, _mockProductRepository.Object);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WithNonExistingCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var categoryId = 2;
            _mockCategoryRepository.Setup(repo => repo.ExistsAsync(categoryId, CancellationToken.None))
                .ReturnsAsync(false);

            var command = new DeleteCategoryCommand(categoryId);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id)
                .WithErrorMessage("Category does not exist.");
        }
    }
}
