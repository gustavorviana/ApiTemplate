namespace ApiTemplate.Application.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
}
