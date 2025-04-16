using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.Commands.Category;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Implementation.CommandHandlers.Category;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Shouldly;

namespace ECommerceNetApp.Service.UnitTest.CommandHandlers.Category
{
    public class UpdateCategoryCommandHandlerTest
    {
        private readonly UpdateCategoryCommandHandler _commandHandler;
        private readonly Mock<ICategoryRepository> _mockRepository;
        private readonly Mock<IValidator<UpdateCategoryCommand>> _mockValidator;

        public UpdateCategoryCommandHandlerTest()
        {
            // Initialize the command handler with necessary dependencies
            _mockRepository = new Mock<ICategoryRepository>();
            _mockValidator = new Mock<IValidator<UpdateCategoryCommand>>();
            _mockValidator.Setup(c => c.ValidateAsync(It.IsAny<UpdateCategoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _commandHandler = new UpdateCategoryCommandHandler(_mockRepository.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithValidData_ShouldUpdateCategory()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 1, Name = "Electronics Updated" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryEntity(categoryDto.Id, "Electronics"));

            _mockRepository.Setup(c => c.UpdateAsync(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name && c.Id == 1), CancellationToken.None))
                .Verifiable();

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);
            await _commandHandler.Handle(updateCategoryCommand, CancellationToken.None);

            // Assert
            _mockRepository.Verify(
                r => r.UpdateAsync(
                    It.Is<CategoryEntity>(c => c.Name == categoryDto.Name),
                    CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var categoryDto = new CategoryDto { Id = 99, Name = "Electronics Updated" };

            _mockRepository.Setup(repo => repo.GetByIdAsync(It.Is<int>(id => id == 1), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CategoryEntity(categoryDto.Id, "Electronics"));

            _mockRepository.Setup(c => c.UpdateAsync(It.Is<CategoryEntity>(c => c.Name == categoryDto.Name && c.Id == 1), CancellationToken.None))
                .Verifiable();

            // Act
            var updateCategoryCommand = new UpdateCategoryCommand(categoryDto.Id, categoryDto.Name, null, null);

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _commandHandler.Handle(updateCategoryCommand, CancellationToken.None);
            });

            // Assert
            _mockRepository.Verify(
                r => r.UpdateAsync(
                    It.Is<CategoryEntity>(c => c.Name == categoryDto.Name),
                    CancellationToken.None),
                Times.Never);
        }
    }
}
