namespace ApiTemplate.Api.Configuration;

public sealed class RateLimitingOptions
{
	public bool Enabled { get; set; } = false;

	public bool UseGlobalLimiterOnly { get; set; } = false;

	public int GlobalRequestsPerMinute { get; set; } = 100;

	public int LoginRequestsPerMinute { get; set; } = 5;

	public int AuthenticatedRequestsPerMinute { get; set; } = 60;

	public int AdminRequestsPerMinute { get; set; } = 120;
}

