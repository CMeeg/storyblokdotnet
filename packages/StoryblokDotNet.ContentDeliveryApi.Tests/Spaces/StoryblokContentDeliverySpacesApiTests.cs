using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Spaces;

public sealed class StoryblokContentDeliverySpacesApiTests
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
		StoryblokContentDeliveryHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliverySpacesApi sut = new(contentDeliveryHttpClient);

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
		StoryblokContentDeliveryHttpClient contentDeliveryHttpClient = new(httpClient, new StoryblokContentDeliveryHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliverySpacesApi sut = new(contentDeliveryHttpClient);

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await sut.RetrieveCurrentSpace(builder => builder.WithToken("builder-token"), TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=builder-token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
		Assert.NotNull(response.Data);
	}
}
