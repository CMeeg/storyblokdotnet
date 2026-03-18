namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiOptions
{
	public IList<StoryblokContentDeliveryHttpClientOptions> Clients { get; } =
	[
		new StoryblokContentDeliveryHttpClientOptions(),
	];

	public StoryblokContentDeliveryApiOptions()
	{
	}

	internal StoryblokContentDeliveryApiOptions(StoryblokContentDeliveryHttpClientOptions options)
	{
		Clients.Clear();

		Clients.Add(options);
	}
}
