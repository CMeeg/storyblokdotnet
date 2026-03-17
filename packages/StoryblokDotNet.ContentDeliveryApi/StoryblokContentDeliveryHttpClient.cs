namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryHttpClient
{
	private readonly HttpClient httpClient;

	public StoryblokContentDeliveryHttpClientOptions Options { get; }

	public Uri BaseAddress => httpClient.BaseAddress!;

	public StoryblokContentDeliveryHttpClient(HttpClient httpClient, StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(httpClient.BaseAddress);

		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		this.httpClient = httpClient;

		Options = resolvedOptions;
	}
}
