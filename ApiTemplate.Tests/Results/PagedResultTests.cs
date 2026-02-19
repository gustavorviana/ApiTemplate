using ApiTemplate.Application.Results;

namespace ApiTemplate.Tests.Results;

public class PagedResultTests
{
	[Fact]
	public void Constructor_SetsPageNumberAndTotalPages()
	{
		var data = new List<int> { 1, 2, 3 };
		var result = new PagedResult<int>(data, pageNumber: 2, totalPages: 5);

		Assert.Equal(2, result.PageNumber);
		Assert.Equal(5, result.TotalPages);
		Assert.Equal(200, result.Status);
		Assert.Equal(data, result.Data);
	}

	[Fact]
	public void Create_CalculatesTotalPagesCorrectly()
	{
		var data = new List<int> { 1, 2, 3 };
		var result = PagedResult<int>.Create(data, pageNumber: 1, pageSize: 10, totalCount: 25);

		Assert.Equal(1, result.PageNumber);
		Assert.Equal(3, result.TotalPages);
		Assert.Equal(data, result.Data);
	}

	[Fact]
	public void Create_WithZeroPageSize_TotalPagesZero()
	{
		var result = PagedResult<int>.Create([], 1, 0, 0);

		Assert.Equal(0, result.TotalPages);
	}

	[Fact]
	public void ImplicitConversion_FromProblemResult_ReturnsErrorWithZeroPagination()
	{
		var problem = new ProblemResult(404, "Not Found");
		PagedResult<string> result = problem;

		Assert.Equal(404, result.Status);
		Assert.Same(problem, result.Problem);
		Assert.Empty(result.Data);
		Assert.Equal(0, result.PageNumber);
		Assert.Equal(0, result.TotalPages);
	}
}
