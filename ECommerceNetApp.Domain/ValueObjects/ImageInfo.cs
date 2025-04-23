namespace ECommerceNetApp.Domain.ValueObjects
{
    public record ImageInfo
    {
#pragma warning disable CA1054 // URI-like parameters should not be strings
        public ImageInfo(string url, string? altText = null)
#pragma warning restore CA1054 // URI-like parameters should not be strings
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(url);

            Url = url;
            AltText = altText ?? string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageInfo"/> class.
        /// Default constructor for ORM purposes.
        /// </summary>
        private ImageInfo()
        {
            Url = string.Empty;
            AltText = string.Empty;
        }

        public string Url { get; init; }

        public string AltText { get; init; }
    }
}
