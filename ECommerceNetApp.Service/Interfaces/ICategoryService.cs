namespace ECommerceNetApp.Service.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();

        Task<CategoryDto?> GetCategoryByIdAsync(int id);

        Task<CategoryDto> AddCategoryAsync(CategoryDto categoryDto);

        Task UpdateCategoryAsync(CategoryDto categoryDto);

        Task DeleteCategoryAsync(int id);
    }
}
