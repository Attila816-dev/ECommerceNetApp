using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class CreateCategoryCommandHandlerTest
    {
        private readonly CreateCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly CategoryMapper _categoryMapper;
        private readonly Mock<IValidator<CreateCategoryCommand>> _mockValidator;

        public CreateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(x => x.CategoryRepository).Returns(_mockRepository.Object);

            _categoryMapper = new CategoryMapper();
            _mockValidator = new Mock<IValidator<CreateCategoryCommand>>();
            _mockValidator.Setup(c => c.ValidateAsync(It.IsAny<CreateCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _commandHandler = new CreateCategoryCommandHandler(_mockUnitOfWork.Object, _categoryMapper, _mockValidator.Object);
        }

        [Fact]
        public async Task AddCategoryAsync_WithValidData_ShouldAddCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Name = "Electronics" };

            _mockRepository.Setup(c => c.AddAsync(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name), CancellationToken.None))
                .Verifiable();

            _mockUnitOfWork.Setup(x => x.CommitAsync(CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _commandHandler.Handle(new CreateCategoryCommand(categoryDto.Name, null, null), CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.AddAsync(
                    It.Is<CategoryEntity>(c => c.Name == categoryDto.Name),
                    CancellationToken.None),
                Times.Once);

            _mockUnitOfWork.Verify(
                r => r.CommitAsync(CancellationToken.None),
                Times.Once);
        }
    }
}
