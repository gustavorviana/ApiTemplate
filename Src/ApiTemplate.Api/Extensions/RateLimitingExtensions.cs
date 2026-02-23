using System.Threading.RateLimiting;
using ApiTemplate.Api.Configuration;

namespace ApiTemplate.Api.Extensions;

public static class RateLimitingExtensions
{
	private const string ConfigurationSectionName = "RateLimiting";

	public const string LoginPolicyName = "LoginPolicy";
	public const string AuthenticatedPolicyName = "AuthenticatedPolicy";
	public const string AdminRoleName = "Admin";

	public static TBuilder AddCustomRateLimiting<TBuilder>(this TBuilder builder)
		where TBuilder : IHostApplicationBuilder
	{
		var options = new RateLimitingOptions();
		builder.Configuration.GetSection(ConfigurationSectionName).Bind(options);

		if (!options.Enabled)
		{
			return builder;
		}

		builder.Services.AddRateLimiter(rateLimiterOptions =>
		{
			rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			if (options.UseGlobalLimiterOnly)
			{
				rateLimiterOptions.GlobalLimiter =
					PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
					{
						var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
							? httpContext.User.Identity!.Name ?? "anonymous"
							: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

						var permitLimit = httpContext.User.IsInRole(AdminRoleName)
							? options.AdminRequestsPerMinute
							: options.GlobalRequestsPerMinute;

						return RateLimitPartition.GetSlidingWindowLimiter(
							partitionKey,
							_ => new SlidingWindowRateLimiterOptions
							{
								PermitLimit = permitLimit,
								Window = TimeSpan.FromMinutes(1),
								SegmentsPerWindow = 1
							});
					});

				return;
			}

			rateLimiterOptions.AddPolicy(LoginPolicyName, httpContext =>
			{
				var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

				return RateLimitPartition.GetFixedWindowLimiter(
					partitionKey,
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = options.LoginRequestsPerMinute,
						Window = TimeSpan.FromMinutes(1)
					});
			});

			rateLimiterOptions.AddPolicy(AuthenticatedPolicyName, httpContext =>
			{
				var partitionKey = httpContext.User.Identity?.IsAuthenticated == true
					? httpContext.User.Identity!.Name ?? "anonymous"
					: "anonymous";

				var permitLimit = httpContext.User.IsInRole(AdminRoleName)
					? options.AdminRequestsPerMinute
					: options.AuthenticatedRequestsPerMinute;

				return RateLimitPartition.GetSlidingWindowLimiter(
					partitionKey,
					_ => new SlidingWindowRateLimiterOptions
					{
						PermitLimit = permitLimit,
						Window = TimeSpan.FromMinutes(1),
						SegmentsPerWindow = 1
					});
			});
		});

		return builder;
	}
}
