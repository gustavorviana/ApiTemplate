using System.ComponentModel.DataAnnotations;

namespace ApiTemplate.Infrastructure.Auth;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(AllowEmptyStrings = false)]
    [MinLength(32, ErrorMessage = "JwtSettings:Secret must be at least 32 characters.")]
    public string Secret { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Issuer { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; set; } = 15;

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
