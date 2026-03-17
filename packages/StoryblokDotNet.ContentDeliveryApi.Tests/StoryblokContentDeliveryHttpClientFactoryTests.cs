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
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu), client.BaseAddress);
		Assert.Equal(string.Empty, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithSpecificRegion_UsesMatchingEndpoint()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Region = StoryblokRegion.Us,
		};

		StoryblokContentDeliveryHttpClient client = sut.Create(options);

		Assert.Equal(StoryblokRegion.Us, client.Options.Region);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Us), client.BaseAddress);
		Assert.Equal(string.Empty, Assert.Single(httpClientFactory.ClientNames));
	}

	[Fact]
	public void Create_WithSameRegion_ReusesTypedClientInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory sut = new(httpClientFactory);
		StoryblokContentDeliveryHttpClientOptions firstOptions = new()
		{
			Region = StoryblokRegion.Canada,
		};
		StoryblokContentDeliveryHttpClientOptions secondOptions = new()
		{
			Region = StoryblokRegion.Canada,
		};

		StoryblokContentDeliveryHttpClient first = sut.Create(firstOptions);
		StoryblokContentDeliveryHttpClient second = sut.Create(secondOptions);

		Assert.Same(first, second);
		Assert.Equal(string.Empty, Assert.Single(httpClientFactory.ClientNames));
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
		Assert.All(httpClientFactory.ClientNames, static clientName => Assert.Equal(string.Empty, clientName));
	}

	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClientFactory(null!));
	}
}
