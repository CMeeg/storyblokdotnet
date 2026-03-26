using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			(IHttpClientFactory)null!,
			StoryblokContentDeliveryNoOpCvCache.Instance));
	}

	[Fact]
	public void Constructor_WithoutClients_ThrowsArgumentNullException()
	{
		RecordingHttpClientFactory httpClientFactory = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(
			(IList<StoryblokContentDeliveryHttpClientOptions>)null!,
			httpClientFactory,
			StoryblokContentDeliveryNoOpCvCache.Instance));
	}

	[Fact]
	public void Constructor_WithoutCvCache_ThrowsArgumentNullException()
	{
		RecordingHttpClientFactory httpClientFactory = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			httpClientFactory,
			null!));
	}

	[Fact]
	public void Constructor_WithHttpClientFactory_UsesDefaultRegion()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiClient sut = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			httpClientFactory,
			StoryblokContentDeliveryNoOpCvCache.Instance);

		Assert.Equal(StoryblokRegion.Eu, sut.Region);
		Assert.Equal("TOKEN", sut.Token);
	}

	[Fact]
	public void Constructor_WithToken_UsesEuRegionAndToken()
	{
		StoryblokContentDeliveryApiClient sut = new("TOKEN");

		Assert.Equal(StoryblokRegion.Eu, sut.Region);
		Assert.Equal("TOKEN", sut.Token);
	}

	[Fact]
	public async Task Constructor_WithCvCache_UsesCvCache()
	{
		RecordingCvCache cvCache = new();
		StoryblokContentDeliveryApiClient sut = new("TOKEN", cvCache);

		await sut.ClearCv(TestContext.Current.CancellationToken);

		Assert.Equal(StoryblokRegion.Eu, cvCache.ClearedRegion);
	}

	[Fact]
	public void Constructor_WithNullToken_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient((string)null!));
	}

	[Fact]
	public void Constructor_WithNullClientOptions_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient((StoryblokContentDeliveryHttpClientOptions)null!));
	}

	[Fact]
	public void Constructor_WithEmptyClients_ThrowsArgumentOutOfRangeException()
	{
		RecordingHttpClientFactory httpClientFactory = new();

		Assert.Throws<ArgumentOutOfRangeException>(() => new StoryblokContentDeliveryApiClient(
			[],
			httpClientFactory,
			StoryblokContentDeliveryNoOpCvCache.Instance));
	}

	[Fact]
	public void Constructor_WithTokenAndHttpClientFactory_UsesFactory()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiClient sut = new("TOKEN", httpClientFactory);

		_ = sut.Spaces();

		Assert.Contains(StoryblokContentDeliveryApiClient.HttpClientName, httpClientFactory.ClientNames);
	}

	[Fact]
	public void Constructor_WithTokenAndHttpClientFactoryFunction_UsesFunction()
	{
		int invocationCount = 0;
		StoryblokContentDeliveryApiClient sut = new("TOKEN", () =>
		{
			invocationCount++;
			return new HttpClient();
		});

		_ = sut.Spaces();

		Assert.Equal(1, invocationCount);
	}

	[Fact]
	public void Constructor_WithClientOptions_UsesOptionsRegionAndToken()
	{
		StoryblokContentDeliveryHttpClientOptions client = new()
		{
			Region = StoryblokRegion.Us,
			Token = "us-token",
		};
		StoryblokContentDeliveryApiClient sut = new(client);

		Assert.Equal(client.Region, sut.Region);
		Assert.Equal(client.Token, sut.Token);
	}

	[Fact]
	public void Constructor_WithClientOptionsAndHttpClientFactory_UsesFactory()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientOptions client = new() { Token = "TOKEN" };
		StoryblokContentDeliveryApiClient sut = new(client, httpClientFactory);

		_ = sut.Spaces();

		Assert.Contains(StoryblokContentDeliveryApiClient.HttpClientName, httpClientFactory.ClientNames);
	}

	[Fact]
	public void Constructor_WithClientOptionsAndHttpClientFactoryFunction_UsesFunction()
	{
		int invocationCount = 0;
		StoryblokContentDeliveryHttpClientOptions client = new() { Token = "TOKEN" };
		StoryblokContentDeliveryApiClient sut = new(client, () =>
		{
			invocationCount++;
			return new HttpClient();
		});

		_ = sut.Spaces();

		Assert.Equal(1, invocationCount);
	}

	[Fact]
	public void Constructor_WithFactoryConfiguredWithMultipleClients_UsesFirstRegisteredClientAsDefault()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		List<StoryblokContentDeliveryHttpClientOptions> clients =
		[
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Us,
				Token = "us-token",
			},
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Eu,
				Token = "eu-token",
			},
		];
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpCvCache.Instance);

		Assert.Equal(clients.First().Region, sut.Region);
		Assert.Equal(clients.First().Token, sut.Token);
	}

	[Fact]
	public void ForRegion_WithSpecificRegion_ReturnsNewClient()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiClient sut = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			httpClientFactory,
			StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.NotNull(usClient);
		Assert.NotSame(sut, usClient);
	}

	[Fact]
	public void ForRegion_WithSameRegion_ReturnsSameInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiClient sut = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			httpClientFactory,
			StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient usClient1 = sut.ForRegion(StoryblokRegion.Us);
		StoryblokContentDeliveryApiClient usClient2 = usClient1.ForRegion(StoryblokRegion.Us);

		Assert.Same(usClient1, usClient2);
	}

	[Fact]
	public void ForRegion_WithRegionSpecificConfiguredToken_UsesTokenForTargetRegion()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		List<StoryblokContentDeliveryHttpClientOptions> clients =
		[
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
		];
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.Equal(clients[0].Token, sut.Token);
		Assert.Equal(clients[1].Token, usClient.Token);
	}

	[Fact]
	public void Constructor_WithHttpClientFactoryFunction_UsesFunctionToCreateClient()
	{
		int invocationCount = 0;
		StoryblokContentDeliveryApiClient sut = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			() =>
			{
				invocationCount++;
				return new HttpClient();
			},
			StoryblokContentDeliveryNoOpCvCache.Instance);

		_ = sut.Spaces();

		Assert.Equal(1, invocationCount);
		Assert.Equal(StoryblokRegion.Eu, sut.Region);
	}

	[Fact]
	public void ForRegion_WithSameRegionRequestedTwice_ReusesTypedClientInstance()
	{
		int invocationCount = 0;
		List<StoryblokContentDeliveryHttpClientOptions> clients =
		[
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Canada,
				Token = "ca-token",
			},
		];
		StoryblokContentDeliveryApiClient sut = new(
			clients,
			() =>
			{
				invocationCount++;
				return new HttpClient();
			},
			StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient first = sut.ForRegion(StoryblokRegion.Canada);
		StoryblokContentDeliveryApiClient second = sut.ForRegion(StoryblokRegion.Canada);

		Assert.Same(first, second);
		Assert.Equal(1, invocationCount);
	}

	[Fact]
	public void ForRegion_WithUnconfiguredRegion_UsesRegionDefaultOptions()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		List<StoryblokContentDeliveryHttpClientOptions> clients =
		[
			new StoryblokContentDeliveryHttpClientOptions { Token = "eu-token" },
		];
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient australiaClient = sut.ForRegion(StoryblokRegion.Australia);

		Assert.Equal(StoryblokRegion.Australia, australiaClient.Region);
		Assert.Equal(string.Empty, australiaClient.Token);
	}

	[Fact]
	public void ForRegion_WithDifferentRegions_ReturnsDistinctTypedClientInstances()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		List<StoryblokContentDeliveryHttpClientOptions> clients =
		[
			new StoryblokContentDeliveryHttpClientOptions { Token = "eu-token" },
		];
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpCvCache.Instance);

		StoryblokContentDeliveryApiClient euClient = sut;
		StoryblokContentDeliveryApiClient australiaClient = sut.ForRegion(StoryblokRegion.Australia);

		Assert.NotSame(euClient, australiaClient);
		Assert.All(httpClientFactory.ClientNames, static clientName => Assert.Equal(StoryblokContentDeliveryApiClient.HttpClientName, clientName));
	}

	[Fact]
	public void Constructor_WithOptionsMutatedAfterConstruction_UsesConstructionTimeSnapshot()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientOptions usClientOptions = new()
		{
			Region = StoryblokRegion.Us,
			Token = "initial-token",
		};
		List<StoryblokContentDeliveryHttpClientOptions> clients = [usClientOptions];
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpCvCache.Instance);

		usClientOptions.Token = "mutated-token";

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.Equal("initial-token", usClient.Token);
	}

	[Fact]
	public async Task ClearCv_WithCurrentRegion_ClearsCacheForCurrentRegion()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		RecordingCvCache cvCache = new();
		StoryblokContentDeliveryApiClient baseClient = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN", Region = StoryblokRegion.Us }],
			httpClientFactory,
			cvCache);
		StoryblokContentDeliveryApiClient sut = baseClient.ForRegion(StoryblokRegion.Us);

		await sut.ClearCv(TestContext.Current.CancellationToken);

		Assert.Equal(StoryblokRegion.Us, cvCache.ClearedRegion);
	}

	private sealed class RecordingCvCache : IStoryblokContentDeliveryCvCache
	{
		public StoryblokRegion? ClearedRegion { get; private set; }

		public Task<long> GetOrCreateCv(
			StoryblokRegion region,
			Func<CancellationToken, Task<long>> valueFactory,
			CancellationToken cancellationToken = default)
		{
			return valueFactory(cancellationToken);
		}

		public Task ClearCv(StoryblokRegion region, CancellationToken cancellationToken = default)
		{
			ClearedRegion = region;
			return Task.CompletedTask;
		}
	}
}
