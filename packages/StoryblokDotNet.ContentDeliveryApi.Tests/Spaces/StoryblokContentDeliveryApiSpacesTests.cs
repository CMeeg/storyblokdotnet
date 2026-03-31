using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Spaces;

public sealed class StoryblokContentDeliveryApiSpacesTests
{
	[Fact]
	public async Task RetrieveCurrentSpace_WithNullQuery_UsesSpacesMeEndpointAndDeserializesResponse()
	{
		const string responseJson = """
		{
		  "space": {
		    "id": 123456,
		    "name": "Storyblok",
		    "domain": "https://www.storyblok.com/",
		    "version": 1544117388,
		    "language_codes": ["de", "es"]
		  }
		}
		""";
		using RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
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
		StoryblokContentDeliveryApiSpaces sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await sut.RetrieveCurrentSpace(cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=configured-token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
		Assert.NotNull(response.Data);
		Assert.Equal(123456, response.Data!.Space.Id);
		Assert.Equal("Storyblok", response.Data.Space.Name);
		Assert.Equal("https://www.storyblok.com/", response.Data.Space.Domain);
		Assert.Equal(1544117388, response.Data.Space.Version);
		Assert.Equal(["de", "es"], response.Data.Space.LanguageCodes);
	}

	[Fact]
	public async Task RetrieveCurrentSpace_WithBuilderAction_UsesBuilderConfiguredToken()
	{
		const string responseJson = """
		{
		  "space": {
		    "id": 1,
		    "name": "Storyblok",
		    "domain": "https://www.storyblok.com/",
		    "version": 1,
		    "language_codes": ["en"]
		  }
		}
		""";
		using RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
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
		StoryblokContentDeliveryApiSpaces sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await sut.RetrieveCurrentSpace(builder => builder.WithToken("builder-token"), cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=builder-token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
		Assert.NotNull(response.Data);
	}

	[Fact]
	public async Task RetrieveCurrentSpace_WithCacheOptions_ForwardsOptionsToHttpClientCache()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		}, cache);
		StoryblokContentDeliveryApiSpaces sut = new(contentDeliveryHttpClient);
		StoryblokContentDeliveryApiCacheEntryOptions cacheOptions = new();
		cacheOptions.Tags.Add("space");

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await sut.RetrieveCurrentSpace(cacheEntryOptions: cacheOptions, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Same(cacheOptions, cache.ReceivedOptions);
	}

	private static HttpResponseMessage CreateJsonResponse()
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
			    "language_codes": ["en"]
			  }
			}
			""", Encoding.UTF8, "application/json"),
		};
	}

	private sealed class RecordingApiCache : IStoryblokContentDeliveryApiCache
	{
		public StoryblokContentDeliveryApiCacheEntryOptions? ReceivedOptions { get; private set; }

		public Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
			StoryblokRegion region,
			StoryblokContentDeliveryRequest request,
			Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
			StoryblokContentDeliveryApiCacheEntryOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			ReceivedOptions = options;
			return valueFactory(cancellationToken);
		}

		public Task Clear(StoryblokRegion region, StoryblokContentDeliveryRequest request, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}

		public Task ClearByTag(string tag, CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}

		public Task ClearAll(CancellationToken cancellationToken = default)
		{
			return Task.CompletedTask;
		}
	}
}
