using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryApiClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiClient(null!));
	}

	[Fact]
	public void Constructor_WithHttpClientFactory_Succeeds()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("TOKEN");
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options);

		StoryblokContentDeliveryApiClient sut = new(factory);

		Assert.Equal(StoryblokRegion.Eu, sut.Region);
	}

	[Fact]
	public void Constructor_WithFactoryConfiguredWithMultipleClients_UsesFirstRegisteredClientAsDefault()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new(new List<StoryblokContentDeliveryHttpClientOptions>
		{
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
		});
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options);

		StoryblokContentDeliveryApiClient sut = new(factory);

		Assert.Equal(StoryblokRegion.Us, sut.Region);
		Assert.Equal("us-token", sut.Token);
	}

	[Fact]
	public void ForRegion_WithSpecificRegion_ReturnsNewClient()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("TOKEN");
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.NotNull(usClient);
		Assert.NotSame(sut, usClient);
	}

	[Fact]
	public void ForRegion_WithSameRegion_ReturnsSameInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("TOKEN");
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient1 = sut.ForRegion(StoryblokRegion.Us);
		StoryblokContentDeliveryApiClient usClient2 = usClient1.ForRegion(StoryblokRegion.Us);

		Assert.Same(usClient1, usClient2);
	}

	[Fact]
	public void ForRegion_WithRegionSpecificConfiguredToken_UsesTokenForTargetRegion()
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
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options);
		StoryblokContentDeliveryApiClient sut = new(factory, StoryblokRegion.Eu);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.Equal("eu-token", sut.Token);
		Assert.Equal("us-token", usClient.Token);
	}

	[Fact]
	public async Task ClearCv_WithCurrentRegion_ClearsCacheForCurrentRegion()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryApiOptions options = new("TOKEN");
		RecordingCvCache cvCache = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory, options, cvCache);
		StoryblokContentDeliveryApiClient sut = new(factory, StoryblokRegion.Us);

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
