using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN" }],
			(IHttpClientFactory)null!,
			StoryblokContentDeliveryNoOpApiCache.Instance));
	}

	[Fact]
	public void Constructor_WithoutClients_ThrowsArgumentNullException()
	{
		RecordingHttpClientFactory httpClientFactory = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(
			(IList<StoryblokContentDeliveryHttpClientOptions>)null!,
			httpClientFactory,
			StoryblokContentDeliveryNoOpApiCache.Instance));
	}

	[Fact]
	public void Constructor_WithoutCache_ThrowsArgumentNullException()
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
			StoryblokContentDeliveryNoOpApiCache.Instance);

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
	public async Task Constructor_WithCache_UsesCache()
	{
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiClient sut = new("TOKEN", cache);
		RetrieveCurrentSpaceRequest request = new(new RetrieveCurrentSpaceQuery());

		await sut.Clear(request, TestContext.Current.CancellationToken);

		Assert.Equal(StoryblokRegion.Eu, cache.ClearedRegion);
		Assert.NotNull(cache.ClearedRequest);
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
			StoryblokContentDeliveryNoOpApiCache.Instance));
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
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpApiCache.Instance);

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
			StoryblokContentDeliveryNoOpApiCache.Instance);

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
			StoryblokContentDeliveryNoOpApiCache.Instance);

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
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpApiCache.Instance);

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
			StoryblokContentDeliveryNoOpApiCache.Instance);

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
			StoryblokContentDeliveryNoOpApiCache.Instance);

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
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpApiCache.Instance);

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
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpApiCache.Instance);

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
		StoryblokContentDeliveryApiClient sut = new(clients, httpClientFactory, StoryblokContentDeliveryNoOpApiCache.Instance);

		usClientOptions.Token = "mutated-token";

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.Equal("initial-token", usClient.Token);
	}

	[Fact]
	public async Task Clear_WithCurrentRegion_ClearsCacheForCurrentRegion()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiClient baseClient = new(
			[new StoryblokContentDeliveryHttpClientOptions { Token = "TOKEN", Region = StoryblokRegion.Us }],
			httpClientFactory,
			cache);
		StoryblokContentDeliveryApiClient sut = baseClient.ForRegion(StoryblokRegion.Us);
		RetrieveCurrentSpaceRequest request = new(new RetrieveCurrentSpaceQuery());

		await sut.Clear(request, TestContext.Current.CancellationToken);

		Assert.Equal(StoryblokRegion.Us, cache.ClearedRegion);
		Assert.Equal(RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath, cache.ClearedRequest!.Path);
		Assert.Contains(cache.ClearedRequest.Query.GetParameters(), parameter => parameter.Key == "token" && parameter.Value == "TOKEN");
	}

	[Fact]
	public async Task ClearByTag_WithTag_DelegatesToCache()
	{
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiClient sut = new("TOKEN", cache);

		await sut.ClearByTag("stories", TestContext.Current.CancellationToken);

		Assert.Equal("stories", cache.ClearedTag);
	}

	[Fact]
	public async Task ClearAll_DelegatesToCache()
	{
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiClient sut = new("TOKEN", cache);

		await sut.ClearAll(TestContext.Current.CancellationToken);

		Assert.Equal(1, cache.ClearAllInvocations);
	}

	private sealed class RecordingApiCache : IStoryblokContentDeliveryApiCache
	{
		public StoryblokRegion? ClearedRegion { get; private set; }
		public StoryblokContentDeliveryRequest? ClearedRequest { get; private set; }
		public string? ClearedTag { get; private set; }
		public int ClearAllInvocations { get; private set; }

		public Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
			StoryblokRegion region,
			StoryblokContentDeliveryRequest request,
			Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
			StoryblokContentDeliveryCacheOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(request);
			ClearedRegion = region;
			return valueFactory(cancellationToken);
		}

		public Task Clear(StoryblokRegion region, StoryblokContentDeliveryRequest request, CancellationToken cancellationToken = default)
		{
			ClearedRegion = region;
			ClearedRequest = request;
			return Task.CompletedTask;
		}

		public Task ClearByTag(string tag, CancellationToken cancellationToken = default)
		{
			ClearedTag = tag;
			return Task.CompletedTask;
		}

		public Task ClearAll(CancellationToken cancellationToken = default)
		{
			ClearAllInvocations++;
			return Task.CompletedTask;
		}
	}
}
