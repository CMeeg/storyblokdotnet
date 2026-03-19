namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryHttpClientFactoryTests
{
	[Fact]
	public void Create_WithoutOptions_UsesEuDefaults()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);

		StoryblokContentDeliveryHttpClient client = sut.Create();

		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
		Assert.Equal(string.Empty, client.Options.Token);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu), client.BaseAddress);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.HttpClientName, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithSpecificRegion_UsesMatchingEndpoint()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Region = StoryblokRegion.Us,
			Token = "us-token",
		};

		StoryblokContentDeliveryHttpClient client = sut.Create(options);

		Assert.Equal(StoryblokRegion.Us, client.Options.Region);
		Assert.Equal("us-token", client.Options.Token);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Us), client.BaseAddress);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.HttpClientName, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithSameRegion_ReusesTypedClientInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);
		StoryblokContentDeliveryHttpClientOptions firstOptions = new()
		{
			Region = StoryblokRegion.Canada,
			Token = "ca-token",
		};
		StoryblokContentDeliveryHttpClientOptions secondOptions = new()
		{
			Region = StoryblokRegion.Canada,
			Token = "ca-token",
		};

		StoryblokContentDeliveryHttpClient first = sut.Create(firstOptions);
		StoryblokContentDeliveryHttpClient second = sut.Create(secondOptions);

		Assert.Same(first, second);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.HttpClientName, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithSameRegionAndDifferentToken_ReusesTypedClientInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);
		StoryblokContentDeliveryHttpClientOptions firstOptions = new()
		{
			Region = StoryblokRegion.Canada,
			Token = "first-token",
		};
		StoryblokContentDeliveryHttpClientOptions secondOptions = new()
		{
			Region = StoryblokRegion.Canada,
			Token = "second-token",
		};

		StoryblokContentDeliveryHttpClient first = sut.Create(firstOptions);
		StoryblokContentDeliveryHttpClient second = sut.Create(secondOptions);

		Assert.Same(first, second);
		Assert.Equal("first-token", first.Options.Token);
		Assert.Equal("first-token", second.Options.Token);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.HttpClientName, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithDifferentRegions_ReturnsDistinctTypedClientInstances()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);

		StoryblokContentDeliveryHttpClient euClient = sut.Create();
		StoryblokContentDeliveryHttpClient australiaClient = sut.Create(new StoryblokContentDeliveryHttpClientOptions
		{
			Region = StoryblokRegion.Australia,
		});

		Assert.NotSame(euClient, australiaClient);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Australia), australiaClient.BaseAddress);
		Assert.Equal(2, httpClientFactory.ClientNames.Count);
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
		StoryblokContentDeliveryApiOptions apiOptions = new();
		apiOptions.Clients.Clear();
		apiOptions.Clients.Add(usClientOptions);
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory, apiOptions);

		usClientOptions.Token = "mutated-token";

		StoryblokContentDeliveryHttpClient client = sut.Create(StoryblokRegion.Us);

		Assert.Equal("initial-token", client.Options.Token);
	}

	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClientFactory((IHttpClientFactory)null!));
	}

	[Fact]
	public void Constructor_WithHttpClientFactoryFunction_UsesFunctionToCreateClient()
	{
		int invocationCount = 0;
		StoryblokContentDeliveryHttpClientFactory sut = new(() =>
		{
			invocationCount++;
			return new HttpClient();
		});

		StoryblokContentDeliveryHttpClient client = sut.Create();

		Assert.Equal(1, invocationCount);
		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu), client.BaseAddress);
	}
}
