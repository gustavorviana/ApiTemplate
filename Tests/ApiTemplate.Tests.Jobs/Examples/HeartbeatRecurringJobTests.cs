using ApiTemplate.Jobs.Examples;
using ApiTemplate.Tests.Jobs.Base;
using ExecutionFlow.Abstractions;
using NSubstitute;

namespace ApiTemplate.Tests.Jobs.Examples;

public class HeartbeatRecurringJobTests : HandlerTestBase
{
    private readonly HeartbeatRecurringJob _handler = new();

    [Fact]
    public async Task HandleAsync_LogsInfoOnce()
    {
        await _handler.HandleAsync(CreateFlowContext(), CancellationToken.None);

        ExecutionLogger.Received(1).Log(
            HandlerLogType.Information,
            Arg.Any<string>(),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task HandleAsync_MessageStartsWithHeartbeat()
    {
        await _handler.HandleAsync(CreateFlowContext(), CancellationToken.None);

        ExecutionLogger.Received(1).Log(
            HandlerLogType.Information,
            Arg.Is<string>(s => s.StartsWith("Heartbeat at ")),
            Arg.Any<object[]>());
    }
}
