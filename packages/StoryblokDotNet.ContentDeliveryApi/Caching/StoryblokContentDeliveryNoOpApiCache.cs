namespace StoryblokDotNet.ContentDeliveryApi.Caching;

internal sealed class StoryblokContentDeliveryNoOpApiCache : IStoryblokContentDeliveryApiCache
{
	public static StoryblokContentDeliveryNoOpApiCache Instance { get; } = new();

	private StoryblokContentDeliveryNoOpApiCache()
	{
	}

	public Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
		StoryblokContentDeliveryCacheEntryOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(valueFactory);

		return valueFactory(cancellationToken);
	}

	public Task Clear(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		return Task.CompletedTask;
	}

	public Task ClearByTag(string tag, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(tag);

		return Task.CompletedTask;
	}

	public Task ClearAll(CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}
}
