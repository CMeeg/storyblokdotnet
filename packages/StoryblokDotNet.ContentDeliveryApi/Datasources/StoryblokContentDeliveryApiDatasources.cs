using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class StoryblokContentDeliveryApiDatasources
{
	private readonly StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient;

	internal StoryblokContentDeliveryApiDatasources(StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient)
	{
		ArgumentNullException.ThrowIfNull(contentDeliveryHttpClient);

		this.contentDeliveryHttpClient = contentDeliveryHttpClient;
	}

	public Task<StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse>> RetrieveSingleDatasource(
		string datasourceId,
		RetrieveSingleDatasourceQuery? query = null,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		RetrieveSingleDatasourceQuery resolvedQuery = query ?? new RetrieveSingleDatasourceQuery();
		RetrieveSingleDatasourceRequest request = new(datasourceId, resolvedQuery);

		return contentDeliveryHttpClient.Get<RetrieveSingleDatasourceResponse>(request, cacheEntryOptions, cancellationToken);
	}

	public Task<StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse>> RetrieveSingleDatasource(
		string datasourceId,
		Action<RetrieveSingleDatasourceQueryBuilder> query,
		StoryblokContentDeliveryApiCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);

		RetrieveSingleDatasourceQueryBuilder builder = new();
		query(builder);

		return RetrieveSingleDatasource(datasourceId, builder.Build(), cacheEntryOptions, cancellationToken);
	}
}
