namespace ApiTemplate.Application.Core.ValueObjects;

public sealed record JwtClaimsContext(Guid UserId, string Name, string Email);
