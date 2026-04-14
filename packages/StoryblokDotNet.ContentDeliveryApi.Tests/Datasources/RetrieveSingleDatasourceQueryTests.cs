using StoryblokDotNet.ContentDeliveryApi.Datasources;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Datasources;

public sealed class RetrieveSingleDatasourceQueryTests
{
	[Fact]
	public void Build_WithTokenSetByBuilder_ReturnsQueryWithToken()
	{
		RetrieveSingleDatasourceQueryBuilder sut = new();

		RetrieveSingleDatasourceQuery result = sut
			.WithToken("preview token")
			.Build();

		Assert.Equal("preview token", result.Token);
	}

	[Fact]
	public void Build_WithCvSetByBuilder_ReturnsQueryWithCv()
	{
		RetrieveSingleDatasourceQuery result = new RetrieveSingleDatasourceQueryBuilder()
			.WithCv(1735815318)
			.Build();

		Assert.Equal(1735815318, result.Cv);
	}

	[Fact]
	public void GetParameters_WithToken_ReturnsTokenParameter()
	{
		RetrieveSingleDatasourceQuery sut = new()
		{
			Token = "preview token",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Single(result);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("preview token", result[0].Value);
	}

	[Fact]
	public void GetParameters_WithTokenAndCv_ReturnsTokenAndCvParameters()
	{
		RetrieveSingleDatasourceQuery sut = new()
		{
			Token = "preview token",
			Cv = 1735815318,
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Equal(2, result.Length);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("preview token", result[0].Value);
		Assert.Equal("cv", result[1].Key);
		Assert.Equal("1735815318", result[1].Value);
	}

	[Fact]
	public void GetParameters_WithoutToken_ReturnsEmptyList()
	{
		RetrieveSingleDatasourceQuery sut = new();

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Empty(result);
	}
}
