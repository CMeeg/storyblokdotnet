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
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);

		StoryblokContentDeliveryApiClient sut = new(factory);

		Assert.NotNull(sut);
	}

	[Fact]
	public void Region_WithSpecificRegion_ReturnsNewClient()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.NotNull(usClient);
		Assert.NotSame(sut, usClient);
	}

	[Fact]
	public void Region_WithNullRegion_ReturnsNewClient()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient fallbackClient = sut.ForRegion(null);

		Assert.NotNull(fallbackClient);
		Assert.NotSame(sut, fallbackClient);
	}

	[Fact]
	public void Region_WithMultipleCalls_ReturnsNewInstanceEachTime()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient1 = sut.ForRegion(StoryblokRegion.Us);
		StoryblokContentDeliveryApiClient usClient2 = sut.ForRegion(StoryblokRegion.Us);

		Assert.NotSame(usClient1, usClient2);
	}

	private sealed class RecordingHttpClientFactory : IHttpClientFactory
	{
		private readonly List<string> clientNames = [];

		public List<string> ClientNames => clientNames;

		public HttpClient CreateClient(string name)
		{
			clientNames.Add(name);
			return new HttpClient();
		}
	}
}
