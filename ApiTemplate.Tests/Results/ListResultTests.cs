using ApiTemplate.Application.Results;

namespace ApiTemplate.Tests.Results;

public class ListResultTests
{
	[Fact]
	public void Constructor_WithData_SetsStatus200WhenNoProblem()
	{
		var items = new List<int> { 1, 2, 3 };
		var result = new ListResult<int>(items);

		Assert.Equal(200, result.Status);
		Assert.Equal(items, result.Data);
		Assert.Null(result.Problem);
	}

	[Fact]
	public void Constructor_WithProblem_UsesProblemStatus()
	{
		var problem = new ProblemResult(500, "Internal Error");
		var result = new ListResult<int>([], problem);

		Assert.Equal(500, result.Status);
		Assert.Empty(result.Data);
		Assert.Same(problem, result.Problem);
	}

	[Fact]
	public void ImplicitConversion_FromProblemResult_ReturnsErrorWithEmptyList()
	{
		var problem = new ProblemResult(400, "Bad Request");
		ListResult<string> result = problem;

		Assert.Equal(400, result.Status);
		Assert.NotNull(result.Data);
		Assert.Empty(result.Data);
		Assert.Same(problem, result.Problem);
	}

	[Fact]
	public void ImplicitConversion_FromList_CreatesSuccessResult()
	{
		var items = new List<string> { "a", "b" };
		ListResult<string> result = items;

		Assert.Equal(200, result.Status);
		Assert.Equal(2, result.Data.Count);
		Assert.Equal("a", result.Data[0]);
		Assert.Null(result.Problem);
	}
}
