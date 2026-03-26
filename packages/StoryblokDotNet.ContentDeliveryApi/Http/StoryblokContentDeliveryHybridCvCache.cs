using Microsoft.Extensions.Caching.Hybrid;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

internal sealed class StoryblokContentDeliveryHybridCvCache : IStoryblokContentDeliveryCvCache
{
	private static readonly TimeSpan CvCacheTtl = TimeSpan.FromHours(24);
	private static readonly HybridCacheEntryOptions CvCacheEntryOptions = new()
	{
		Expiration = CvCacheTtl,
		LocalCacheExpiration = CvCacheTtl,
	};

	private readonly HybridCache cache;

	private static string CreateCacheKey(StoryblokRegion region)
	{
		return $"StoryblokContentDeliveryApi:cv:{region}";
	}

	public StoryblokContentDeliveryHybridCvCache(HybridCache cache)
	{
		ArgumentNullException.ThrowIfNull(cache);

		this.cache = cache;
	}

	public async Task<long> GetOrCreateCv(
		StoryblokRegion region,
		Func<CancellationToken, Task<long>> valueFactory,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(valueFactory);

		return await cache.GetOrCreateAsync(
			CreateCacheKey(region),
			valueFactory,
			static async (factory, cancel) => await factory(cancel).ConfigureAwait(false),
			CvCacheEntryOptions,
			cancellationToken: cancellationToken).ConfigureAwait(false);
	}

	public async Task ClearCv(StoryblokRegion region, CancellationToken cancellationToken = default)
	{
		await cache.RemoveAsync(CreateCacheKey(region), cancellationToken).ConfigureAwait(false);
	}
}
