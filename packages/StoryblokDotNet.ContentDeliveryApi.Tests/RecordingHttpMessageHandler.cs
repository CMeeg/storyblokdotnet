namespace StoryblokDotNet.ContentDeliveryApi.Tests;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
	private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

	public Uri? RequestUri { get; private set; }
	public List<Uri> RequestUris { get; } = [];

	public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
	{
		ArgumentNullException.ThrowIfNull(responseFactory);

		this.responseFactory = responseFactory;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (request.RequestUri is Uri requestUri)
		{
			RequestUri = requestUri;
			RequestUris.Add(requestUri);
		}

		return Task.FromResult(responseFactory(request));
	}
}
