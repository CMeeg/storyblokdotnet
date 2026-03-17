namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryHttpClientTests
{
	[Fact]
	public void Constructor_WithoutOptions_CreatesDefaultOptions()
	{
		HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};

		StoryblokContentDeliveryHttpClient client = new(httpClient);

		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
	}
}
