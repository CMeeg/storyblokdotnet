using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiOptionsTests
{
	[Fact]
	public void Constructor_WithoutParameters_UsesCvCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new();

		Assert.True(sut.UseCvCache);
	}

	[Fact]
	public void Constructor_WithClient_UsesCvCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new StoryblokContentDeliveryHttpClientOptions());

		Assert.True(sut.UseCvCache);
	}

	[Fact]
	public void Constructor_WithClients_UsesCvCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new List<StoryblokContentDeliveryHttpClientOptions>
		{
			new StoryblokContentDeliveryHttpClientOptions(),
		});

		Assert.True(sut.UseCvCache);
	}
}
