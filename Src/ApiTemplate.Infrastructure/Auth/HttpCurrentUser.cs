using System.Security.Claims;
using ApiTemplate.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ApiTemplate.Infrastructure.Auth;

public class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            var claim = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? accessor.HttpContext?.User.FindFirst("sub");

            return claim is not null && Guid.TryParse(claim.Value, out var id)
                ? id
                : null;
        }
    }
}
