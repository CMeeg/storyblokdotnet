using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Http;

public sealed class StoryblokContentDeliveryHttpClientFactoryTests
{
	[Fact]
	public void Create_WithoutRegion_UsesDefaultConfiguredClient()
	{
		const string token = "TOKEN";

		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new(token);
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, options);

		StoryblokContentDeliveryHttpClient client = sut.Create();

		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
		Assert.Equal(token, client.Options.Token);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu), client.BaseAddress);
	}

	[Fact]
	public void Create_WithSpecificRegion_UsesMatchingEndpoint()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new(new List<StoryblokContentDeliveryHttpClientOptions>
		{
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Eu,
				Token = "eu-token",
			},
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Us,
				Token = "us-token",
			},
		});
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, options);

		StoryblokContentDeliveryHttpClient client = sut.Create(StoryblokRegion.Us);

		Assert.Equal(StoryblokRegion.Us, client.Options.Region);
		Assert.Equal("us-token", client.Options.Token);
	}

	[Fact]
	public void Create_WithSameRegion_ReusesTypedClientInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new(new List<StoryblokContentDeliveryHttpClientOptions>
		{
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Canada,
				Token = "ca-token",
			},
		});
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, options);

		StoryblokContentDeliveryHttpClient first = sut.Create(StoryblokRegion.Canada);
		StoryblokContentDeliveryHttpClient second = sut.Create(StoryblokRegion.Canada);

		Assert.Same(first, second);
	}

	[Fact]
	public void Create_WithUnconfiguredRegion_UsesRegionDefaultOptions()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("eu-token");
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, options);

		StoryblokContentDeliveryHttpClient client = sut.Create(StoryblokRegion.Australia);

		Assert.Equal(StoryblokRegion.Australia, client.Options.Region);
		Assert.Equal(string.Empty, client.Options.Token);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Australia), client.BaseAddress);
	}

	[Fact]
	public void Create_WithDifferentRegions_ReturnsDistinctTypedClientInstances()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("eu-token");
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, options);

		StoryblokContentDeliveryHttpClient euClient = sut.Create();
		StoryblokContentDeliveryHttpClient australiaClient = sut.Create(StoryblokRegion.Australia);

		Assert.NotSame(euClient, australiaClient);
		Assert.All(httpClientFactory.ClientNames, static clientName => Assert.Equal(StoryblokContentDeliveryHttpClientFactory.HttpClientName, clientName));
	}

	[Fact]
	public void Create_WithOptionsMutatedAfterFactoryConstruction_UsesConstructionTimeSnapshot()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientOptions usClientOptions = new()
		{
			Region = StoryblokRegion.Us,
			Token = "initial-token",
		};
		StoryblokContentDeliveryApiOptions apiOptions = new(usClientOptions);
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, apiOptions);

		usClientOptions.Token = "mutated-token";

		StoryblokContentDeliveryHttpClient client = sut.Create(StoryblokRegion.Us);

		Assert.Equal("initial-token", client.Options.Token);
	}

	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		StoryblokContentDeliveryApiOptions options = new(new StoryblokContentDeliveryHttpClientOptions());

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClientFactory((IHttpClientFactory)null!, options));
	}

	[Fact]
	public void Constructor_WithHttpClientFactoryFunction_UsesFunctionToCreateClient()
	{
		int invocationCount = 0;
		StoryblokContentDeliveryApiOptions options = new("TOKEN");
		StoryblokContentDeliveryHttpClientFactory sut = new(() =>
		{
			invocationCount++;
			return new HttpClient();
		}, options);

		StoryblokContentDeliveryHttpClient client = sut.Create();

		Assert.Equal(1, invocationCount);
		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
	}
}
