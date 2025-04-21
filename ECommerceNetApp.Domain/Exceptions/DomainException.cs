using System.Net;

namespace ECommerceNetApp.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public DomainException()
        {
        }

        public DomainException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HttpStatusCode? StatusCode { get; private set; }
    }
}
