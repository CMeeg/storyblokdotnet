using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class StoryblokContentDeliveryApiTags
{
	private readonly StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient;

	internal StoryblokContentDeliveryApiTags(StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse>> RetrieveMultipleTags(
		RetrieveMultipleTagsQuery? query = null,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		RetrieveMultipleTagsQuery resolvedQuery = query ?? new RetrieveMultipleTagsQuery();
		RetrieveMultipleTagsRequest request = new(resolvedQuery);

		return contentDeliveryHttpClient.Get<RetrieveMultipleTagsResponse>(request, cacheEntryOptions, cancellationToken);
	}

	public Task<StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse>> RetrieveMultipleTags(
		Action<RetrieveMultipleTagsQueryBuilder> query,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveMultipleTagsQueryBuilder builder = new();
		query(builder);

		return RetrieveMultipleTags(builder.Build(), cacheEntryOptions, cancellationToken);
	}
}
