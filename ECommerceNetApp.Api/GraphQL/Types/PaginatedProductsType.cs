using ECommerceNetApp.Service.DTO;
using HotChocolate.Types;

namespace ECommerceNetApp.Api.GraphQL.Types
{
    public class PaginatedProductsType : ObjectType<PaginationResult<ProductDto>>
    {
        protected override void Configure(IObjectTypeDescriptor<PaginationResult<ProductDto>> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            descriptor.Description("Represents paginated products result");

            descriptor.Field(p => p.Items)
                .Description("The products in the current page");

            descriptor.Field(p => p.TotalCount)
                .Description("The total number of products");

            descriptor.Field(p => p.HasNextPage)
                .Description("Indicates if there are more pages");

            descriptor.Field(p => p.HasPreviousPage)
                .Description("Indicates if there are previous pages");
        }
    }
}
