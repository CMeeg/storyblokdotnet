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
}
