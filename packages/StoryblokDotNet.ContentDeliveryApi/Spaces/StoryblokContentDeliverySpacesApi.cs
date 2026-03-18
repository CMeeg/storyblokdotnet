namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokContentDeliverySpacesApi
{
	private const string RetrieveCurrentSpacePath = "/spaces/me";

	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;

	public StoryblokRegion Region => contentDeliveryHttpClient.Options.Region;
	public string Token => contentDeliveryHttpClient.Options.Token;

	internal StoryblokContentDeliverySpacesApi(StoryblokContentDeliveryHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<RetrieveCurrentSpaceResponse?> RetrieveCurrentSpace(RetrieveCurrentSpaceQuery? query = null)
	{
		RetrieveCurrentSpaceQuery resolvedQuery = query ?? new RetrieveCurrentSpaceQuery();

		return contentDeliveryHttpClient.Get<RetrieveCurrentSpaceResponse>(RetrieveCurrentSpacePath, resolvedQuery);
	}

	public Task<RetrieveCurrentSpaceResponse?> RetrieveCurrentSpace(Action<RetrieveCurrentSpaceQueryBuilder> query)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveCurrentSpaceQueryBuilder builder = new();
		query(builder);

		return RetrieveCurrentSpace(builder.Build());
	}
}
