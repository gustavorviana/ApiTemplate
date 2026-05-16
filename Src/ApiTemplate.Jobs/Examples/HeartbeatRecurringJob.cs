using ExecutionFlow.Abstractions;
using ExecutionFlow.Attributes;

namespace ApiTemplate.Jobs.Examples;

[Recurring("* * * * *")]
public sealed class HeartbeatRecurringJob : IHandler
{
    public Task HandleAsync(FlowContext context, CancellationToken ct)
    {
        context.Log.Info($"Heartbeat at {DateTimeOffset.UtcNow:O}");
        return Task.CompletedTask;
    }
}
