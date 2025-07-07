using HotChocolate.Types;

namespace ECommerceNetApp.Api.GraphQL.InputTypes
{
    // Input types for category mutation
    public class UpdateCategoryInputType : InputObjectType<UpdateCategoryInput>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UpdateCategoryInput> descriptor)
        {
            ArgumentNullException.ThrowIfNull(descriptor);
            descriptor.Description("Input for updating an existing category");

            descriptor.Field(c => c.Id)
                .Description("The ID of the category to update");

            descriptor.Field(c => c.Name)
                .Description("The name of the category");

            descriptor.Field(c => c.ImageUrl)
                .Description("The URL of the category image");

            descriptor.Field(c => c.ParentCategoryId)
                .Description("The ID of the parent category");
        }
    }
}
