using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Datasources;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Datasources;

public sealed class StoryblokContentDeliveryApiDatasourcesTests
{
	[Fact]
	public async Task RetrieveSingleDatasource_WithNullQuery_UsesDatasourceEndpointAndDeserializesResponse()
	{
		const string responseJson = """
		{
		  "datasource": {
		    "id": 989,
		    "name": "Sizes",
		    "slug": "sizes",
		    "dimensions": [
		      {
		        "id": 1,
		        "name": "English",
		        "entry_value": "en",
		        "datasource_id": 989,
		        "created_at": "2025-01-01T00:00:00.000Z",
		        "updated_at": "2025-01-02T00:00:00.000Z"
		      },
		      {
		        "id": 2,
		        "name": "German",
		        "entry_value": "de",
		        "datasource_id": 989,
		        "created_at": "2025-01-01T00:00:00.000Z",
		        "updated_at": "2025-01-02T00:00:00.000Z"
		      }
		    ]
		  },
		  "cv": 1735815318
		}
		""";
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseJson, Encoding.UTF8, "application/json"),
			});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiDatasources sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse> response = await sut.RetrieveSingleDatasource(
			"sizes",
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/datasources/sizes?token=configured-token&cv=1", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
		Assert.NotNull(response.Data);
		Assert.Equal(1735815318, response.Data!.Cv);
		Assert.Equal(989, response.Data.Datasource.Id);
		Assert.Equal("Sizes", response.Data.Datasource.Name);
		Assert.Equal("sizes", response.Data.Datasource.Slug);
		Assert.Equal(2, response.Data.Datasource.Dimensions.Count);
		Assert.Equal("en", response.Data.Datasource.Dimensions[0].EntryValue);
		Assert.Equal("de", response.Data.Datasource.Dimensions[1].EntryValue);
	}

	[Fact]
	public async Task RetrieveSingleDatasource_WithBuilderAction_UsesBuilderConfiguredToken()
	{
		using RecordingHttpMessageHandler handler = new(request => CreateDatasourceJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiDatasources sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse> response = await sut.RetrieveSingleDatasource(
			"catalog/sizes",
			builder => builder.WithToken("builder-token").WithCv(17),
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/datasources/catalog%2Fsizes?token=builder-token&cv=17", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
	}

	[Fact]
	public async Task RetrieveSingleDatasource_WithCacheOptions_ForwardsOptionsToHttpClientCache()
	{
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: CreateDatasourceJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		}, cache);
		StoryblokContentDeliveryApiDatasources sut = new(contentDeliveryHttpClient);
		StoryblokContentDeliveryApiCacheEntryOptions cacheOptions = new();
		cacheOptions.Tags.Add("datasource");

		StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse> response = await sut.RetrieveSingleDatasource(
			"sizes",
			cacheEntryOptions: cacheOptions,
			cancellationToken: TestContext.Current.CancellationToken);
		CacheCall cvPrefetchCall = Assert.Single(cache.CacheCalls, call => string.Equals(call.RequestPath, RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath, StringComparison.OrdinalIgnoreCase));
		CacheCall datasourceCall = Assert.Single(cache.CacheCalls, call => string.Equals(call.RequestPath, "/datasources/sizes", StringComparison.OrdinalIgnoreCase));

		Assert.True(response.IsSuccess);
		Assert.Same(cacheOptions, datasourceCall.Options);
		Assert.NotSame(cacheOptions, cvPrefetchCall.Options);
		Assert.NotNull(cvPrefetchCall.Options);
		Assert.Contains(StoryblokContentDeliveryApiHttpClient.CvCacheTag, cvPrefetchCall.Options!.Tags);
		Assert.NotNull(handler.RequestUri);
		Assert.Contains("cv=1", handler.RequestUri!.Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task RetrieveSingleDatasource_WithNullBuilderAction_ThrowsArgumentNullException()
	{
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: CreateDatasourceJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiDatasources sut = new(contentDeliveryHttpClient);

		await Assert.ThrowsAsync<ArgumentNullException>(() => sut.RetrieveSingleDatasource(
			"sizes",
			(Action<RetrieveSingleDatasourceQueryBuilder>)null!,
			cancellationToken: TestContext.Current.CancellationToken));
	}

	private static bool IsSpacesRequest(HttpRequestMessage request)
	{
		return request.RequestUri?.AbsolutePath.Contains(
			RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath.TrimStart('/'),
			StringComparison.OrdinalIgnoreCase) == true;
	}

	private static HttpResponseMessage CreateDatasourceJsonResponse()
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("""
			{
			  "datasource": {
			    "id": 1,
			    "name": "Sizes",
			    "slug": "sizes",
			    "dimensions": [
			      {
			        "id": 1,
			        "name": "English",
			        "entry_value": "en",
			        "datasource_id": 1,
			        "created_at": "2025-01-01T00:00:00.000Z",
			        "updated_at": "2025-01-02T00:00:00.000Z"
			      }
			    ]
			  },
			  "cv": 1735815318
			}
			""", Encoding.UTF8, "application/json"),
		};
	}

	private static HttpResponseMessage CreateSpacesJsonResponse()
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("""
			{
			  "space": {
			    "id": 1,
			    "name": "Storyblok",
			    "domain": "https://www.storyblok.com/",
			    "version": 1,
			    "language_codes": []
			  }
			}
			""", Encoding.UTF8, "application/json"),
		};
	}

}
