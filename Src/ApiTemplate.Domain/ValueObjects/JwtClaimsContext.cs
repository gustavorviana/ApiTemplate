namespace ApiTemplate.Domain.ValueObjects;

public sealed record JwtClaimsContext(Guid UserId, string Name, string Email);
