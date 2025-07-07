namespace ECommerceNetApp.Api.GraphQL.InputTypes
{
    // Input types for category mutation
    public class UpdateCategoryInput
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
