using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Category;
using ECommerceNetApp.Service.Queries.Product;

namespace ECommerceNetApp.Api.GraphQL.Types
{
    public class CategoryType : ObjectType<CategoryDto>
    {
        protected override void Configure(IObjectTypeDescriptor<CategoryDto> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);

            descriptor.Description("Represents a product category");

            descriptor.Field(c => c.Id)
                .Description("The unique identifier of the category");

            descriptor.Field(c => c.Name)
                .Description("The name of the category");

            descriptor.Field(c => c.ImageUrl)
                .Description("The URL of the category image");

            descriptor.Field(c => c.ParentCategoryId)
                .Description("The ID of the parent category");

            descriptor.Field("products")
                .Description("Products in this category")
                .Type<ListType<ProductType>>()
                .Resolve(async context =>
                {
                    var dispatcher = context.Service<IDispatcher>();
                    var categoryId = context.Parent<CategoryDto>().Id;
                    var query = new GetProductsByCategoryQuery(categoryId);
                    return await dispatcher.SendQueryAsync<GetProductsByCategoryQuery, IEnumerable<ProductDto>>(query, context.RequestAborted).ConfigureAwait(false);
                });

            descriptor.Field("subCategories")
                .Description("Sub-categories of this category")
                .Type<ListType<CategoryType>>()
                .Resolve(async context =>
                {
                    var dispatcher = context.Service<IDispatcher>();
                    var categoryId = context.Parent<CategoryDto>().Id;
                    var query = new GetCategoriesByParentCategoryIdQuery(categoryId);
                    return await dispatcher.SendQueryAsync<GetCategoriesByParentCategoryIdQuery, IEnumerable<CategoryDto>>(query, context.RequestAborted).ConfigureAwait(false);
                });
        }
    }
}
