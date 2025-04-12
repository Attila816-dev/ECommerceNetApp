namespace ECommerceNetApp.Domain.ValueObjects
{
    public class ImageInfo : IEquatable<ImageInfo>
    {
#pragma warning disable CA1054 // URI-like parameters should not be strings
        public ImageInfo(string url, string altText)
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
        }

        public string? Url { get; }

        public string? AltText { get; }

        public bool Equals(ImageInfo? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Url == other.Url && AltText == other.AltText;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ImageInfo);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Url, AltText);
        }
    }
}
