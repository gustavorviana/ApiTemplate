using System.ComponentModel.DataAnnotations;

namespace ApiTemplate.Api.Configuration;

public sealed class RateLimitingOptions
{
	public const string SectionName = "RateLimiting";

	public bool Enabled { get; set; } = false;

	public bool UseGlobalLimiterOnly { get; set; } = false;

	[Range(1, int.MaxValue)]
	public int GlobalRequestsPerMinute { get; set; } = 100;

	[Range(1, int.MaxValue)]
	public int LoginRequestsPerMinute { get; set; } = 5;

	[Range(1, int.MaxValue)]
	public int AuthenticatedRequestsPerMinute { get; set; } = 60;

	[Range(1, int.MaxValue)]
	public int AdminRequestsPerMinute { get; set; } = 120;
}
