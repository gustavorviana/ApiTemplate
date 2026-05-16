using ExecutionFlow.Abstractions;
using NSubstitute;

namespace ApiTemplate.Tests.Jobs.Base;

public abstract class HandlerTestBase : TestBase
{
    protected readonly IExecutionLogger ExecutionLogger;

    protected HandlerTestBase()
    {
        ExecutionLogger = Substitute.For<IExecutionLogger>();
    }

    protected FlowContext<T> CreateFlowContext<T>(T data)
        => new([], ExecutionLogger, data, null!);

    protected FlowContext CreateFlowContext()
        => new([], ExecutionLogger);
}
