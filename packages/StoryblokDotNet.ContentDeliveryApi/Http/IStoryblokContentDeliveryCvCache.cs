namespace StoryblokDotNet.ContentDeliveryApi.Http;

public interface IStoryblokContentDeliveryCvCache
{
	Task<long> GetOrCreateCv(
		StoryblokRegion region,
		Func<CancellationToken, Task<long>> valueFactory,
		CancellationToken cancellationToken = default);

	Task ClearCv(
		StoryblokRegion region,
		CancellationToken cancellationToken = default);
}
