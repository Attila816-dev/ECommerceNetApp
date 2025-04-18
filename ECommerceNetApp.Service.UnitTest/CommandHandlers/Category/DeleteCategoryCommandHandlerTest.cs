using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class DeleteCategoryCommandHandlerTest
    {
        private readonly DeleteCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IValidator<DeleteCategoryCommand>> _mockValidator;

        public DeleteCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockValidator = new Mock<IValidator<DeleteCategoryCommand>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<DeleteCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult());
            _commandHandler = new DeleteCategoryCommandHandler(_mockCategoryRepository.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task DeleteExistingCategory_RemovesCategory()
        {
            // Arrange
            var category = new CategoryEntity(1, "test-category");

            _mockCategoryRepository.Setup(r => r.ExistsAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(true);

            _mockCategoryRepository.Setup(r => r.DeleteAsync(category.Id, CancellationToken.None))
                .Verifiable();

            // Act
            await _commandHandler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

            // Assert
            _mockCategoryRepository.Verify(
                r => r.DeleteAsync(It.Is<int>(c => c == category.Id), CancellationToken.None),
                Times.Once);
        }
    }
}
