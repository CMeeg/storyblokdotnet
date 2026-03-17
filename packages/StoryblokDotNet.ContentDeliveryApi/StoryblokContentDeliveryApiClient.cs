using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiClient
{
	private readonly StoryblokContentDeliveryHttpClientFactory httpClientFactory;
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;
	private readonly StoryblokRegion region;

	public StoryblokRegion Region => region;

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

		var clientOptions = region is StoryblokRegion resolvedRegion
			? new StoryblokContentDeliveryHttpClientOptions
			{
				Region = resolvedRegion,
			}
			: new StoryblokContentDeliveryHttpClientOptions();

		this.region = clientOptions.Region;

		contentDeliveryHttpClient = httpClientFactory.Create(clientOptions);
	}

	public StoryblokContentDeliveryApiClient ForRegion(StoryblokRegion region)
	{
		if (region == this.region)
		{
			return this;
		}

		return new StoryblokContentDeliveryApiClient(httpClientFactory, region);
	}

	public StoryblokContentDeliverySpacesApi Spaces()
	{
		return new StoryblokContentDeliverySpacesApi(contentDeliveryHttpClient);
	}
}
