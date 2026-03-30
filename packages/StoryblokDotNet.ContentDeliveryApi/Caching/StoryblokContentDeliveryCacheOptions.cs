using Microsoft.Extensions.Caching.Hybrid;

namespace StoryblokDotNet.ContentDeliveryApi.Caching;

public sealed class StoryblokContentDeliveryCacheOptions
{
	public HybridCacheEntryOptions? EntryOptions { get; set; }

	public IList<string> Tags { get; } = [];
}
