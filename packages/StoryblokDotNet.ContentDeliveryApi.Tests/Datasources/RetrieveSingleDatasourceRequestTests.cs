using StoryblokDotNet.ContentDeliveryApi.Datasources;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Datasources;

public sealed class RetrieveSingleDatasourceRequestTests
{
	[Fact]
	public void Constructor_WithNullDatasourceId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new RetrieveSingleDatasourceRequest(null!, new RetrieveSingleDatasourceQuery()));

		Assert.Equal("datasourceId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithEmptyDatasourceId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new RetrieveSingleDatasourceRequest(string.Empty, new RetrieveSingleDatasourceQuery()));

		Assert.Equal("datasourceId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithWhitespaceDatasourceId_ThrowsArgumentException()
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new RetrieveSingleDatasourceRequest("   ", new RetrieveSingleDatasourceQuery()));

		Assert.Equal("datasourceId", exception.ParamName);
	}

	[Fact]
	public void Constructor_WithDatasourceIdContainingReservedCharacters_EscapesPathSegment()
	{
		RetrieveSingleDatasourceRequest result = new("products/sizes?x=1", new RetrieveSingleDatasourceQuery());

		Assert.Equal("/datasources/products%2Fsizes%3Fx%3D1", result.Path);
	}
}
