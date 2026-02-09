namespace ApiTemplate.Application.Core.Exceptions
{
    public class DomainException(string message, int statusCode) : Exception(message)
    {
        public int StatusCode { get; set; } = statusCode;
    }
}
