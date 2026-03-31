using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryApiHttpClientOptions
{
	public StoryblokRegion Region { get; set; } = StoryblokRegion.Eu;

	public string Token { get; set; } = string.Empty;

	public StoryblokContentDeliveryApiCacheOptions Cache { get; set; } = new StoryblokContentDeliveryApiCacheOptions();
}
