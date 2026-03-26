namespace StoryblokDotNet.ContentDeliveryApi;

public class StoryblokContentDeliveryRequest
{
	public string Path { get; }

	public StoryblokContentDeliveryQuery Query { get; }

	public StoryblokContentDeliveryRequest(string path, StoryblokContentDeliveryQuery query)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(query);

		Path = path;
		Query = query;
	}
}
