using HotChocolate.Types;

namespace ECommerceNetApp.Api.GraphQL.InputTypes
{
    // Input types for category mutation
    public class CreateCategoryInputType : InputObjectType<CreateCategoryInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateCategoryInput> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            descriptor.Description("Input for creating a new category");

            descriptor.Field(c => c.Name)
                .Description("The name of the category");

            descriptor.Field(c => c.ImageUrl)
                .Description("The URL of the category image");

            descriptor.Field(c => c.ParentCategoryId)
                .Description("The ID of the parent category");
        }
    }
}
