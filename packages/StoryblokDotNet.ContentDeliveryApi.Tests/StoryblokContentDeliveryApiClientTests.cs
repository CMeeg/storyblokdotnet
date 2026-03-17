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

		Assert.Equal(StoryblokRegion.Eu, sut.Region);
	}

	[Fact]
	public void ForRegion_WithSpecificRegion_ReturnsNewClient()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient = sut.ForRegion(StoryblokRegion.Us);

		Assert.NotNull(usClient);
		Assert.NotSame(sut, usClient);
	}

	[Fact]
	public void ForRegion_WithSameRegion_ReturnsSameInstance()
	{
		RecordingHttpClientFactory httpClientFactory = new();
		StoryblokContentDeliveryHttpClientFactory factory = new(httpClientFactory);
		StoryblokContentDeliveryApiClient sut = new(factory);

		StoryblokContentDeliveryApiClient usClient1 = sut.ForRegion(StoryblokRegion.Us);
		StoryblokContentDeliveryApiClient usClient2 = usClient1.ForRegion(StoryblokRegion.Us);

		Assert.Same(usClient1, usClient2);
	}
}
