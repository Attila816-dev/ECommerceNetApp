using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
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
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IValidator<DeleteCategoryCommand>> _mockValidator;

        public DeleteCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.CategoryRepository).Returns(_mockCategoryRepository.Object);

            _mockValidator = new Mock<IValidator<DeleteCategoryCommand>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<DeleteCategoryCommand>(), CancellationToken.None))
                .ReturnsAsync(new ValidationResult());
            _commandHandler = new DeleteCategoryCommandHandler(_mockUnitOfWork.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task DeleteExistingCategory_RemovesCategory()
        {
            // Arrange
            var category = CategoryEntity.Create("test-category", null, null, 1);

            _mockCategoryRepository.Setup(r => r.ExistsAsync(category.Id, CancellationToken.None))
                .ReturnsAsync(true);

            _mockCategoryRepository.Setup(r => r.DeleteAsync(category.Id, CancellationToken.None))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _commandHandler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

            // Assert
            _mockCategoryRepository.Verify(
                r => r.DeleteAsync(It.Is<int>(c => c == category.Id), CancellationToken.None),
                Times.Once);

            _mockUnitOfWork.Verify(
                x => x.CommitAsync(CancellationToken.None),
                Times.Once);
        }
    }
}
