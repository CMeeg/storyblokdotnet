using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;
using StoryblokDotNet.ContentDeliveryApi.Tags;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Tags;

public sealed class StoryblokContentDeliveryApiTagsTests
{
	[Fact]
	public async Task RetrieveMultipleTags_WithNullQuery_UsesTagsEndpointAndDeserializesResponse()
	{
		const string responseJson = """
		{
		  "tags": [
		    { "name": "featured", "taggings_count": 5 },
		    { "name": "news", "taggings_count": 12 }
		  ]
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
		StoryblokContentDeliveryApiTags sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse> response = await sut.RetrieveMultipleTags(cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/tags?token=configured-token&cv=1", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
		Assert.NotNull(response.Data);
		Assert.Equal(2, response.Data!.Tags.Count);
		Assert.Equal("featured", response.Data.Tags[0].Name);
		Assert.Equal(5, response.Data.Tags[0].TaggingsCount);
		Assert.Equal("news", response.Data.Tags[1].Name);
		Assert.Equal(12, response.Data.Tags[1].TaggingsCount);
	}

	[Fact]
	public async Task RetrieveMultipleTags_WithStartsWithQuery_IncludesStartsWithParameter()
	{
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: CreateTagsJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiTags sut = new(contentDeliveryHttpClient);

		await sut.RetrieveMultipleTags(
			new RetrieveMultipleTagsQuery { StartsWith = "blog/" },
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Contains("starts_with=blog%2F", handler.RequestUri!.Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task RetrieveMultipleTags_WithDraftVersion_IncludesDraftVersionParameter()
	{
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: CreateTagsJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiTags sut = new(contentDeliveryHttpClient);

		await sut.RetrieveMultipleTags(
			new RetrieveMultipleTagsQuery { Version = StoryblokVersion.Draft },
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Contains("version=draft", handler.RequestUri!.Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task RetrieveMultipleTags_WithBuilderAction_UsesBuilderConfiguredParameters()
	{
		using RecordingHttpMessageHandler handler = new(request => IsSpacesRequest(request)
			? CreateSpacesJsonResponse()
			: CreateTagsJsonResponse());
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryApiTags sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse> response = await sut.RetrieveMultipleTags(
			builder => builder.WithToken("builder-token").WithStartsWith("blog/"),
			cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Contains("token=builder-token", handler.RequestUri!.Query, StringComparison.Ordinal);
		Assert.Contains("starts_with=blog%2F", handler.RequestUri!.Query, StringComparison.Ordinal);
		Assert.True(response.IsSuccess);
	}

	private static bool IsSpacesRequest(HttpRequestMessage request)
	{
		return request.RequestUri?.AbsolutePath.Contains(
			RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath.TrimStart('/'),
			StringComparison.OrdinalIgnoreCase) == true;
	}

	private static HttpResponseMessage CreateTagsJsonResponse()
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("""
			{
			  "tags": [
			    { "name": "featured", "taggings_count": 3 }
			  ]
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
