using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiClient
{
	private readonly StoryblokContentDeliveryHttpClientFactory httpClientFactory;
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;
	private readonly StoryblokRegion? region;

	public StoryblokRegion? Region => region;

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientFactory httpClientFactory)
		: this(httpClientFactory, null)
	{
	}

	internal StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientFactory httpClientFactory,
		StoryblokRegion? region)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);

		this.httpClientFactory = httpClientFactory;
		this.region = region;

		contentDeliveryHttpClient = region is StoryblokRegion resolvedRegion
			? httpClientFactory.Create(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = resolvedRegion,
			})
			: httpClientFactory.Create();
	}

	public StoryblokContentDeliveryApiClient ForRegion(StoryblokRegion? region)
	{
		return new StoryblokContentDeliveryApiClient(httpClientFactory, region);
	}

	public StoryblokContentDeliverySpacesApi Spaces()
	{
		return new StoryblokContentDeliverySpacesApi(contentDeliveryHttpClient);
	}
}
