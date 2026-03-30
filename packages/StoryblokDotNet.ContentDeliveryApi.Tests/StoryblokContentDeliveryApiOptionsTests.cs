using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiOptionsTests
{
	[Fact]
	public void Constructor_WithoutParameters_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new();

		Assert.True(sut.UseCache);
	}

	[Fact]
	public void Constructor_WithClient_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new StoryblokContentDeliveryHttpClientOptions());

		Assert.True(sut.UseCache);
	}

	[Fact]
	public void Constructor_WithClients_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new List<StoryblokContentDeliveryHttpClientOptions>
		{
			new StoryblokContentDeliveryHttpClientOptions(),
		});

		Assert.True(sut.UseCache);
	}
}
