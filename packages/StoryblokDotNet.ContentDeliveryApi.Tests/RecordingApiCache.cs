using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

internal sealed class RecordingApiCache : IStoryblokContentDeliveryApiCache
{
	public List<CacheCall> CacheCalls { get; } = [];
	public StoryblokRegion? ClearedRegion { get; private set; }
	public StoryblokContentDeliveryRequest? ClearedRequest { get; private set; }
	public string? ClearedTag { get; private set; }
	public int ClearAllInvocations { get; private set; }

	public Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
		StoryblokContentDeliveryApiCacheEntryOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		CacheCalls.Add(new CacheCall(request.Path, options));
		return valueFactory(cancellationToken);
	}

	public Task Clear(StoryblokRegion region, StoryblokContentDeliveryRequest request, CancellationToken cancellationToken = default)
	{
		ClearedRegion = region;
		ClearedRequest = request;
		return Task.CompletedTask;
	}

	public Task ClearByTag(string tag, CancellationToken cancellationToken = default)
	{
		ClearedTag = tag;
		return Task.CompletedTask;
	}

	public Task ClearAll(CancellationToken cancellationToken = default)
	{
		ClearAllInvocations++;
		return Task.CompletedTask;
	}
}

internal sealed record CacheCall(string RequestPath, StoryblokContentDeliveryApiCacheEntryOptions? Options);
