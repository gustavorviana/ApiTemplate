using ApiTemplate.Contracts.Events;
using ApiTemplate.Jobs.Examples;
using ApiTemplate.Tests.Jobs.Base;
using ExecutionFlow.Abstractions;
using NSubstitute;

namespace ApiTemplate.Tests.Jobs.Examples;

public class WeatherForecastCreatedJobTests : HandlerTestBase
{
    private readonly WeatherForecastCreatedJob _handler = new();

    [Fact]
    public async Task HandleAsync_LogsInfoOnce()
    {
        var evt = new WeatherForecastCreatedEvent(Guid.NewGuid(), new DateOnly(2026, 5, 16), 21, "Mild");

        await _handler.HandleAsync(CreateFlowContext(evt), CancellationToken.None);

        ExecutionLogger.Received(1).Log(
            HandlerLogType.Information,
            Arg.Any<string>(),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task HandleAsync_MessageContainsEventFields()
    {
        var id = Guid.NewGuid();
        var evt = new WeatherForecastCreatedEvent(id, new DateOnly(2026, 5, 16), 21, "Mild");

        await _handler.HandleAsync(CreateFlowContext(evt), CancellationToken.None);

        ExecutionLogger.Received(1).Log(
            HandlerLogType.Information,
            Arg.Is<string>(s =>
                s.Contains(id.ToString())
                && s.Contains("21")
                && s.Contains("Mild")),
            Arg.Any<object[]>());
    }

    [Fact]
    public async Task HandleAsync_NullSummary_RendersFallback()
    {
        var evt = new WeatherForecastCreatedEvent(Guid.NewGuid(), new DateOnly(2026, 5, 16), 5, null);

        await _handler.HandleAsync(CreateFlowContext(evt), CancellationToken.None);

        ExecutionLogger.Received(1).Log(
            HandlerLogType.Information,
            Arg.Is<string>(s => s.Contains("<none>")),
            Arg.Any<object[]>());
    }
}
