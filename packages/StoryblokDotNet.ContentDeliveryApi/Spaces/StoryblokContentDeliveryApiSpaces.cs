using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokContentDeliveryApiSpaces
{
	private readonly StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient;

	internal StoryblokContentDeliveryApiSpaces(StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		RetrieveCurrentSpaceQuery? query = null,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		RetrieveCurrentSpaceQuery resolvedQuery = query ?? new RetrieveCurrentSpaceQuery();
		RetrieveCurrentSpaceRequest request = new(resolvedQuery);

		return contentDeliveryHttpClient.Get<RetrieveCurrentSpaceResponse>(request, cacheEntryOptions, cancellationToken);
	}

	public Task<StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse>> RetrieveCurrentSpace(
		Action<RetrieveCurrentSpaceQueryBuilder> query,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveCurrentSpaceQueryBuilder builder = new();
		query(builder);

		return RetrieveCurrentSpace(builder.Build(), cacheEntryOptions, cancellationToken);
	}
}
