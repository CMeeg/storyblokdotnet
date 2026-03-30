using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryHttpClientOptions
{
	public StoryblokRegion Region { get; set; } = StoryblokRegion.Eu;

	public string Token { get; set; } = string.Empty;

	public StoryblokContentDeliveryCacheOptions Cache { get; set; } = new StoryblokContentDeliveryCacheOptions();
}
