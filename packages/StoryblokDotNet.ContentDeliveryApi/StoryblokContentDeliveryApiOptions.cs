namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiOptions
{
	public IList<StoryblokContentDeliveryHttpClientOptions> Clients { get; } =
	[
		new StoryblokContentDeliveryHttpClientOptions(),
	];
}
