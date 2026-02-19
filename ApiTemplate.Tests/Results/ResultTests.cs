using ApiTemplate.Application.Results;

namespace ApiTemplate.Tests.Results;

public class ResultTests
{
	[Fact]
	public void Constructor_SetsStatusAndProblem()
	{
		var problem = new ProblemResult(404, "Not Found", "https://example.com/not-found");
		var result = new Result(404, problem);

		Assert.Equal(404, result.Status);
		Assert.Same(problem, result.Problem);
	}

	[Fact]
	public void Constructor_WithNullProblem_StatusOnly()
	{
		var result = new Result(204);

		Assert.Equal(204, result.Status);
		Assert.Null(result.Problem);
	}
}
