using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryHttpClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClient_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClient(null!));
	}

	[Fact]
	public void Constructor_WithHttpClientWithoutBaseAddress_ThrowsArgumentNullException()
	{
		using HttpClient httpClient = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClient(httpClient));
	}

	[Fact]
	public void Constructor_WithoutOptions_CreatesDefaultOptions()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};

		StoryblokContentDeliveryHttpClient client = new(httpClient);

		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
		Assert.Equal(string.Empty, client.Options.Token);
	}

	[Fact]
	public void Constructor_WithExplicitOptions_UsesProvidedOptions()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Region = StoryblokRegion.China,
			Token = "my-token",
		};

		StoryblokContentDeliveryHttpClient client = new(httpClient, options);

		Assert.Same(options, client.Options);
		Assert.Equal(StoryblokRegion.China, client.Options.Region);
		Assert.Equal("my-token", client.Options.Token);
	}

	[Fact]
	public async Task Get_WithSerializedQuery_UsesBasePathAndQueryString()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient);
		RetrieveCurrentSpaceQuery query = new()
		{
			Token = "my token",
		};

		_ = await client.Get<object>("/spaces/me", query);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=my%20token", handler.RequestUri!.AbsoluteUri);
	}

	[Fact]
	public async Task Get_WithEmptyQueryToken_UsesClientToken()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Token = "configured-token",
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, options);
		RetrieveCurrentSpaceQuery query = new();

		_ = await client.Get<object>("/spaces/me", query);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=configured-token", handler.RequestUri!.AbsoluteUri);
	}

	private static HttpResponseMessage CreateJsonResponse(string json)
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json"),
		};
	}
}
