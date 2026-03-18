using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Spaces;

public sealed class RetrieveCurrentSpaceQueryTests
{
	[Fact]
	public void Build_WithTokenSetByBuilder_ReturnsQueryWithToken()
	{
		RetrieveCurrentSpaceQueryBuilder sut = new();

		RetrieveCurrentSpaceQuery result = sut
			.WithToken("preview token")
			.Build();

		Assert.Equal("preview token", result.Token);
	}

	[Fact]
	public void Build_FromBuilderInterface_ReturnsRetrieveCurrentSpaceQuery()
	{
		RetrieveCurrentSpaceQueryBuilder sut = new RetrieveCurrentSpaceQueryBuilder()
			.WithToken("preview token");

		RetrieveCurrentSpaceQuery result = sut.Build();

		Assert.Equal("preview token", result.Token);
	}

	[Fact]
	public void GetParameters_WithToken_ReturnsTokenParameter()
	{
		RetrieveCurrentSpaceQuery sut = new()
		{
			Token = "preview token",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Single(result);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("preview token", result[0].Value);
	}

	[Fact]
	public void GetParameters_WithoutToken_ReturnsEmptyList()
	{
		RetrieveCurrentSpaceQuery sut = new();

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Empty(result);
	}
}
