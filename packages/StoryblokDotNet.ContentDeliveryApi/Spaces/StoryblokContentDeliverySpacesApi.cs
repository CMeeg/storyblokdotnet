using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokContentDeliverySpacesApi
{
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;

	internal StoryblokContentDeliverySpacesApi(StoryblokContentDeliveryHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		RetrieveCurrentSpaceQuery? query = null,
		StoryblokContentDeliveryCacheEntryOptions? cacheOptions = null,
		CancellationToken cancellationToken = default)
	{
		RetrieveCurrentSpaceQuery resolvedQuery = query ?? new RetrieveCurrentSpaceQuery();
		RetrieveCurrentSpaceRequest request = new(resolvedQuery);

		return contentDeliveryHttpClient.Get<RetrieveCurrentSpaceResponse>(request, cacheOptions, cancellationToken);
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		Action<RetrieveCurrentSpaceQueryBuilder> query,
		StoryblokContentDeliveryCacheEntryOptions? cacheOptions = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveCurrentSpaceQueryBuilder builder = new();
		query(builder);

		return RetrieveCurrentSpace(builder.Build(), cacheOptions, cancellationToken);
	}
}
