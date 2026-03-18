using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiClient
{
	private readonly StoryblokContentDeliveryHttpClientFactory httpClientFactory;
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;

	public StoryblokRegion Region => contentDeliveryHttpClient.Options.Region;
	public string Token => contentDeliveryHttpClient.Options.Token;

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

		contentDeliveryHttpClient = region is StoryblokRegion resolvedRegion
			? httpClientFactory.Create(resolvedRegion)
			: httpClientFactory.Create();
	}

	public StoryblokContentDeliveryApiClient ForRegion(StoryblokRegion region)
	{
		if (region == Region)
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
