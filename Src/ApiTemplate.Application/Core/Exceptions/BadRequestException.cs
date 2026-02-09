namespace ApiTemplate.Application.Core.Exceptions
{
    public class BadRequestException(string message = "Request body could not be read properly.") : DomainException(message, 400);
}
