using Microsoft.Extensions.Caching.Hybrid;

namespace StoryblokDotNet.ContentDeliveryApi.Caching;

public sealed class StoryblokContentDeliveryApiCacheEntryOptions
{
	public TimeSpan? Expiration { get; set; }

	public HybridCacheEntryFlags? Flags { get; set; }

	public TimeSpan? LocalCacheExpiration { get; set; }

	public IList<string> Tags { get; } = [];
}
