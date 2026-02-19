using ApiTemplate.Application.Results;

namespace ApiTemplate.Tests.Results;

public class ResultOfTTests
{
	[Fact]
	public void Constructor_SetsStatusDataAndProblem()
	{
		var data = new { Id = 1, Name = "Test" };
		var result = new Result<object>(200, data);

		Assert.Equal(200, result.Status);
		Assert.Same(data, result.Data);
		Assert.Null(result.Problem);
	}

	[Fact]
	public void ImplicitConversion_FromT_Returns200WithData()
	{
		var data = "success";
		Result<string> result = data;

		Assert.Equal(200, result.Status);
		Assert.Equal("success", result.Data);
		Assert.Null(result.Problem);
	}

	[Fact]
	public void ImplicitConversion_FromProblemResult_ReturnsErrorResult()
	{
		var problem = new ProblemResult(404, "Not Found");
		Result<string> result = problem;

		Assert.Equal(404, result.Status);
		Assert.Null(result.Data);
		Assert.Same(problem, result.Problem);
	}
}
