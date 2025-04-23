using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Domain.ValueObjects;
using ECommerceNetApp.Persistence.Interfaces.ProductCatalog;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using ECommerceNetApp.Service.Implementation.Mappers.Category;
using Moq;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class CreateCategoryCommandHandlerTest
    {
        private readonly CreateCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly Mock<IProductCatalogUnitOfWork> _mockUnitOfWork;
        private readonly CategoryMapper _categoryMapper;

        public CreateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IProductCatalogUnitOfWork>();
            _mockUnitOfWork.Setup(x => x.CategoryRepository).Returns(_mockRepository.Object);

            _categoryMapper = new CategoryMapper();
            _commandHandler = new CreateCategoryCommandHandler(_mockUnitOfWork.Object, _categoryMapper);
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

        [Fact]
        public async Task Handle_WithParentCategory_CorrectlyAssignsParent()
        {
            // Arrange
            var parentCategoryId = 42;
            var command = new CreateCategoryCommand("Test Category", "image.jpg", parentCategoryId);
            var parentCategory = new CategoryEntity(parentCategoryId, "Parent Category", new ImageInfo("parent.jpg"));
            var categoryEntity = new CategoryEntity(123, "Test Category", new ImageInfo("image.jpg"), parentCategory);

            _mockUnitOfWork
                .Setup(u => u.CategoryRepository.GetByIdAsync(
                    parentCategoryId,
                    It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(parentCategory);

            _mockUnitOfWork
                .Setup(u => u.CategoryRepository.AddAsync(categoryEntity, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            var result = await _commandHandler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(
                u => u.CategoryRepository.GetByIdAsync(
                    parentCategoryId,
                    It.IsAny<Func<IQueryable<CategoryEntity>, IQueryable<CategoryEntity>>?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(
                u => u.CategoryRepository.AddAsync(
                    It.Is<CategoryEntity>(c => c.Name == command.Name && c.ParentCategory == parentCategory),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
