namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiClient
{
	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientFactory httpClientFactory,
		StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);

		ContentDeliveryHttpClient = httpClientFactory.Create(options);
	}

	public StoryblokContentDeliveryHttpClient ContentDeliveryHttpClient { get; }
}
