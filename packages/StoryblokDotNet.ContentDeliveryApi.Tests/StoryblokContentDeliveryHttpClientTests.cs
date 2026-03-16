namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryHttpClientTests
{
	[Fact]
	public void Constructor_WithoutOptions_CreatesDefaultOptions()
	{
		HttpClient httpClient = new();

		StoryblokContentDeliveryHttpClient client = new(httpClient);

		Assert.Equal(StoryblokRegion.Eu, client.Options.Region);
	}
}
