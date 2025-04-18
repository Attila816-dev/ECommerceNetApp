using ECommerceNetApp.Persistence.Interfaces;
using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces.Mappers.Category;
using ECommerceNetApp.Service.Queries.Category;
using MediatR;

namespace ECommerceNetApp.Service.Implementation.QueryHandlers.Category
{
    public class GetCategoryByIdQueryHandler(
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            ICategoryMapper categoryMapper) : IRequestHandler<GetCategoryByIdQuery, CategoryDetailDto?>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryMapper _categoryMapper = categoryMapper;

        public async Task<CategoryDetailDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
            if (category == null)
            {
                return null;
            }

            var subcategories = await _categoryRepository.GetByParentCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            var products = await _productRepository.GetProductsByCategoryIdAsync(category.Id, cancellationToken).ConfigureAwait(false);
            return _categoryMapper.MapToCategoryDetailDto(category, subcategories, products);
        }
    }
}
