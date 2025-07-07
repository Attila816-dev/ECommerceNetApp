using HotChocolate.Types;

namespace ECommerceNetApp.Api.GraphQL.InputTypes
{
    // Input types for product mutation
    public class CreateProductInputType : InputObjectType<CreateProductInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateProductInput> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            descriptor.Description("Input for creating a new product");

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
        }
    }
}
