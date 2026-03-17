namespace StoryblokDotNet.ContentDeliveryApi.Tests;

internal sealed class RecordingHttpClientFactory : IHttpClientFactory
{
	private readonly List<string> clientNames = [];

	public IReadOnlyList<string> ClientNames => clientNames;

	public HttpClient CreateClient(string name)
	{
		clientNames.Add(name);
		return new HttpClient();
	}
}
