using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiOptionsTests
{
	[Fact]
	public void Constructor_WithoutParameters_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new();

		Assert.True(sut.Cache.UseCache);
		Assert.Equal(StoryblokContentDeliveryApiCacheOptions.DefaultCvTtl, sut.Cache.CvTtl);
	}

	[Fact]
	public void Constructor_WithClient_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new StoryblokContentDeliveryApiHttpClientOptions());

		Assert.True(sut.Cache.UseCache);
		Assert.Equal(StoryblokContentDeliveryApiCacheOptions.DefaultCvTtl, sut.Cache.CvTtl);
	}

	[Fact]
	public void Constructor_WithClients_UsesCacheByDefault()
	{
		StoryblokContentDeliveryApiOptions sut = new(new List<StoryblokContentDeliveryApiHttpClientOptions>
		{
			new StoryblokContentDeliveryApiHttpClientOptions(),
		});

		Assert.True(sut.Cache.UseCache);
		Assert.Equal(StoryblokContentDeliveryApiCacheOptions.DefaultCvTtl, sut.Cache.CvTtl);
	}
}
