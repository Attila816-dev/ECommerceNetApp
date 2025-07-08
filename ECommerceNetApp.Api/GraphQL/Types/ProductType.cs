using ECommerceNetApp.Service.DTO;
using ECommerceNetApp.Service.Interfaces;
using ECommerceNetApp.Service.Queries.Category;

namespace ECommerceNetApp.Api.GraphQL.Types
{
    public class ProductType : ObjectType<ProductDto>
    {
        protected override void Configure(IObjectTypeDescriptor<ProductDto> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            descriptor.Description("Represents a product");

            descriptor.Field(p => p.Id)
                .Description("The unique identifier of the product");

            descriptor.Field(p => p.Name)
                .Description("The name of the product");

            descriptor.Field(p => p.Description)
                .Description("The description of the product");

            descriptor.Field(p => p.ImageUrl)
                .Description("The URL of the product image");

            descriptor.Field(p => p.CategoryId)
                .Description("The ID of the category this product belongs to");

            descriptor.Field(p => p.Price)
                .Description("The price of the product");

            descriptor.Field(p => p.Currency)
                .Description("The currency of the product price");

            descriptor.Field(p => p.Amount)
                .Description("The available amount/stock of the product");

            descriptor.Field("category")
                .Description("The category this product belongs to")
                .Type<CategoryDetailType>()
                .Resolve(async context =>
                {
                    var dispatcher = context.Service<IDispatcher>();
                    var categoryId = context.Parent<ProductDto>().CategoryId;
                    var query = new GetCategoryByIdQuery(categoryId);
                    return await dispatcher.SendQueryAsync<GetCategoryByIdQuery, CategoryDetailDto?>(query, context.RequestAborted).ConfigureAwait(false);
                });
        }
    }
}
