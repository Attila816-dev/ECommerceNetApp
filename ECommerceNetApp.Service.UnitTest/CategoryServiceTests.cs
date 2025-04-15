// using ECommerceNetApp.Domain.Entities;
// using ECommerceNetApp.Persistence.Interfaces;
// using ECommerceNetApp.Service.DTO;
// using ECommerceNetApp.Service.Implementation;
// using Moq;

// namespace ECommerceNetApp.Service.UnitTest
// {
//    public class CategoryServiceTests
//    {
//        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
//        private readonly CategoryService _categoryService;

// public CategoryServiceTests()
//        {
//            _mockCategoryRepository = new Mock<ICategoryRepository>();
//            _categoryService = new CategoryService(_mockCategoryRepository.Object);
//        }

// [Fact]
//        public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
//        {
//            // Arrange
//            var categories = new List<Category>
//            {
//                new Category(1, "Electronics"),
//                new Category(2, "Books"),
//            };

// _mockCategoryRepository.Setup(repo => repo.GetAllAsync())
//                .ReturnsAsync(categories);

// // Act
//            var result = await _categoryService.GetAllCategoriesAsync();

// // Assert
//            Assert.Equal(2, result.Count());
//            Assert.Contains(result, c => c.Id == 1 && c.Name == "Electronics");
//            Assert.Contains(result, c => c.Id == 2 && c.Name == "Books");
//        }

// [Fact]
//        public async Task GetCategoryByIdAsync_WithValidId_ShouldReturnCategory()
//        {
//            // Arrange
//            var category = new Category(1, "Electronics");

// _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1))
//                .ReturnsAsync(category);

// // Act
//            var result = await _categoryService.GetCategoryByIdAsync(1);

// // Assert
//            Assert.NotNull(result);
//            Assert.Equal(1, result.Id);
//            Assert.Equal("Electronics", result.Name);
//        }

// [Fact]
//        public async Task AddCategoryAsync_WithValidData_ShouldAddCategory()
//        {
//            // Arrange
//            var categoryDto = new CategoryDto { Name = "Electronics" };
//            var category = new Category(1, "Electronics");

// _mockCategoryRepository.Setup(repo => repo.AddAsync(It.IsAny<Category>()))
//                .ReturnsAsync(category);

// // Act
//            var result = await _categoryService.AddCategoryAsync(categoryDto);

// // Assert
//            Assert.NotNull(result);
//            Assert.Equal(1, result.Id);
//            Assert.Equal("Electronics", result.Name);

// _mockCategoryRepository.Verify(repo => repo.AddAsync(It.IsAny<Category>()), Times.Once);
//        }

// [Fact]
//        public async Task UpdateCategoryAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
//        {
//            // Arrange
//            var categoryDto = new CategoryDto { Id = 1, Name = "Electronics" };

// _mockCategoryRepository.Setup(repo => repo.GetByIdAsync(1))
//                .ReturnsAsync((Category?)null);

// // Act & Assert
//            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
//                _categoryService.UpdateCategoryAsync(categoryDto));
//        }
//    }
// }
