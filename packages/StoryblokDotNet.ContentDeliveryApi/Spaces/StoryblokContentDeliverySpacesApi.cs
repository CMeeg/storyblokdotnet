using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokContentDeliverySpacesApi
{
	private const string RetrieveCurrentSpacePath = "/spaces/me";

	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;

	internal StoryblokContentDeliverySpacesApi(StoryblokContentDeliveryHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		RetrieveCurrentSpaceQuery? query = null,
		CancellationToken cancellationToken = default)
	{
		RetrieveCurrentSpaceQuery resolvedQuery = query ?? new RetrieveCurrentSpaceQuery();
		StoryblokContentDeliveryRequest request = new(RetrieveCurrentSpacePath, resolvedQuery);

		return contentDeliveryHttpClient.Get<RetrieveCurrentSpaceResponse>(request, cancellationToken);
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		Action<RetrieveCurrentSpaceQueryBuilder> query,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveCurrentSpaceQueryBuilder builder = new();
		query(builder);

		return RetrieveCurrentSpace(builder.Build(), cancellationToken);
	}
}
