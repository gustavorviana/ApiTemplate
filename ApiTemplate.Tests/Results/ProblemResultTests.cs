using ApiTemplate.Application.Results;

namespace ApiTemplate.Tests.Results;

public class ProblemResultTests
{
	[Fact]
	public void Constructor_SetsStatusTypeAndTitle()
	{
		var result = new ProblemResult(404, "Not Found", "https://example.com/not-found");

		Assert.Equal(404, result.Status);
		Assert.Equal("https://example.com/not-found", result.Type);
		Assert.Equal("Not Found", result.Title);
		Assert.NotNull(result.Extensions);
		Assert.Empty(result.Extensions);
	}

	[Fact]
	public void Constructor_EmptyType_DefaultsToAboutBlank()
	{
		var result = new ProblemResult(400, "Bad Request", "");

		Assert.Equal("about:blank", result.Type);
	}

	[Fact]
	public void Constructor_EmptyTitle_DefaultsToError()
	{
		var result = new ProblemResult(500, "  ");

		Assert.Equal("Error", result.Title);
	}

	[Fact]
	public void Constructor_WithExtensions_StoresNonStandardMembers()
	{
		var extensions = new Dictionary<string, object?>
		{
			["detail"] = "Item not found",
			["customKey"] = "customValue"
		};
		var result = new ProblemResult(404, "Not Found", "about:blank", extensions);

		Assert.Single(result.Extensions);
		Assert.True(result.Extensions.ContainsKey("customKey"));
		Assert.Equal("customValue", result.Extensions["customKey"]);
		Assert.False(result.Extensions.ContainsKey("detail"));
	}

	[Fact]
	public void Constructor_NullExtensions_ReturnsEmptyExtensions()
	{
		var result = new ProblemResult(400, "Bad Request");

		Assert.NotNull(result.Extensions);
		Assert.Empty(result.Extensions);
	}
}
