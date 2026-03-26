namespace StoryblokDotNet.ContentDeliveryApi.Http;

internal sealed class StoryblokContentDeliveryNoOpCvCache : IStoryblokContentDeliveryCvCache
{
	public static StoryblokContentDeliveryNoOpCvCache Instance { get; } = new();

	private StoryblokContentDeliveryNoOpCvCache()
	{
	}

	public Task<long> GetOrCreateCv(
		StoryblokRegion region,
		Func<CancellationToken, Task<long>> valueFactory,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(valueFactory);

		return valueFactory(cancellationToken);
	}

	public Task ClearCv(StoryblokRegion region, CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}
}
