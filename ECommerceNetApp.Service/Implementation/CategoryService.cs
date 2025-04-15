using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;

namespace ECommerceNetApp.Service.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync().ConfigureAwait(false);
            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);
            return category != null ? MapToDto(category) : null;
        }

        public async Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDto)
        {
            ValidateCategory(categoryDto);

            Category? parentCategory = null;

            if (categoryDto.ParentCategoryId != null)
            {
                parentCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id).ConfigureAwait(false);

                if (parentCategory == null)
                {
                    throw new KeyNotFoundException($"Parent Category with ID {categoryDto.ParentCategoryId} not found.");
                }
            }

            var category = MapToDomain(categoryDto, parentCategory);
            var result = await _categoryRepository.AddAsync(category).ConfigureAwait(false);

            return MapToDto(result);
        }

        public async Task UpdateCategoryAsync(CategoryDto categoryDto)
        {
            ValidateCategory(categoryDto);

            var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id).ConfigureAwait(false);

            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {categoryDto.Id} not found.");
            }

            existingCategory.UpdateName(categoryDto.Name);
            existingCategory.UpdateImage(categoryDto.ImageUrl);

            if (existingCategory.ParentCategoryId != categoryDto.ParentCategoryId)
            {
                if (categoryDto.ParentCategoryId == null)
                {
                    existingCategory.UpdateParentCategory(null);
                }
                else
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(categoryDto.ParentCategoryId.Value).ConfigureAwait(false);

                    if (parentCategory == null)
                    {
                        throw new KeyNotFoundException($"ParentCategory with ID {categoryDto.ParentCategoryId} not found.");
                    }

                    existingCategory.UpdateParentCategory(parentCategory);
                }
            }

            await _categoryRepository.UpdateAsync(existingCategory).ConfigureAwait(false);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var existingCategory = await _categoryRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }

            await _categoryRepository.DeleteAsync(id).ConfigureAwait(false);
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
            };
        }

        private static Category MapToDomain(CategoryDto dto, Category? parentCategory)
        {
            return new Category(dto.Name, dto.ImageUrl, parentCategory);
        }

        private static void ValidateCategory(CategoryDto category)
        {
            ArgumentNullException.ThrowIfNull(category);

            if (string.IsNullOrEmpty(category.Name))
            {
                throw new ArgumentException("Category name is required.", nameof(category));
            }

            if (category.Name.Length > 50)
            {
                throw new ArgumentException("Category name cannot exceed 50 characters.", nameof(category));
            }
        }
    }
}
