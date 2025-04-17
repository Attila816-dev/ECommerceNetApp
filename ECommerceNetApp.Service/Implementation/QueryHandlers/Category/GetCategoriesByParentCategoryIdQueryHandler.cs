using ECommerceNetApp.Domain.Entities;
using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoriesByParentCategoryIdQueryHandler : IRequestHandler<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetCategoriesByParentCategoryIdQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesByParentCategoryIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            IEnumerable<CategoryEntity> categories = await _categoryRepository.GetByParentCategoryIdAsync(request.ParentCategoryId, cancellationToken).ConfigureAwait(false);
            return categories.Select(c => MapCategoryDto(c));
        }

        private static CategoryDto MapCategoryDto(CategoryEntity category)
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
    }
}
