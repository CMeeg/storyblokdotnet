namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryHttpClient
{
	public StoryblokContentDeliveryHttpClient(HttpClient httpClient, StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(httpClient);

		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		HttpClient = httpClient;
		Options = resolvedOptions;
	}

	public HttpClient HttpClient { get; }

	public StoryblokContentDeliveryHttpClientOptions Options { get; }
}
