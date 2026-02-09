namespace ApiTemplate.Application.Core.Exceptions
{
    public class UnauthorizedException(string message = "Invalid Credentials") : DomainException(message, 401);
}
