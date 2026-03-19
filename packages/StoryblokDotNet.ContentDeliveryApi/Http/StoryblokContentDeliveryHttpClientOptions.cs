namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryHttpClientOptions
{
	public StoryblokRegion Region { get; set; } = StoryblokRegion.Eu;

	public string Token { get; set; } = string.Empty;
}
