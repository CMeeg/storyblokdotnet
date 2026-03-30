namespace StoryblokDotNet.ContentDeliveryApi.Caching;

public interface IStoryblokContentDeliveryApiCache
{
	Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
		StoryblokContentDeliveryCacheEntryOptions? options = null,
		CancellationToken cancellationToken = default);

	Task Clear(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken = default);

	Task ClearByTag(string tag, CancellationToken cancellationToken = default);

	Task ClearAll(CancellationToken cancellationToken = default);
}
