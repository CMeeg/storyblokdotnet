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
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetClientName(StoryblokRegion.Eu), Assert.Single(httpClientFactory.ClientNames));
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
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetClientName(StoryblokRegion.Us), Assert.Single(httpClientFactory.ClientNames));
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
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.GetClientName(StoryblokRegion.Canada), Assert.Single(httpClientFactory.ClientNames));
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
		Assert.Contains(StoryblokContentDeliveryHttpClientFactory.GetClientName(StoryblokRegion.Eu), httpClientFactory.ClientNames);
		Assert.Contains(StoryblokContentDeliveryHttpClientFactory.GetClientName(StoryblokRegion.Australia), httpClientFactory.ClientNames);
	}

	[Fact]
	public void Constructor_WithoutHttpClientFactory_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClientFactory(null!));
	}

	private sealed class RecordingHttpClientFactory : IHttpClientFactory
	{
		private readonly List<string> clientNames = new();

		public List<string> ClientNames => clientNames;

		public HttpClient CreateClient(string name)
		{
			clientNames.Add(name);
			return new HttpClient();
		}
	}
}
