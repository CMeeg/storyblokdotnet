namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokContentDeliverySpacesApi
{
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;

	public StoryblokRegion Region => contentDeliveryHttpClient.Options.Region;
	public string Token => contentDeliveryHttpClient.Options.Token;

	internal StoryblokContentDeliverySpacesApi(StoryblokContentDeliveryHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public object RetrieveCurrentSpace(object query)
	{
		ArgumentNullException.ThrowIfNull(query);
		_ = contentDeliveryHttpClient;

		throw new NotImplementedException();
	}
}
