using System.Net;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryError
{
	public HttpStatusCode? StatusCode { get; init; }

	public StoryblokContentDeliveryErrorCategory Category { get; init; }

	public string Message { get; init; } = string.Empty;

	public string? Details { get; init; }

	public TimeSpan? RetryAfter { get; init; }
}

public enum StoryblokContentDeliveryErrorCategory
{
	Unknown = 0,
	BadRequest,
	Unauthorized,
	NotFound,
	Validation,
	RateLimited,
	ServerError,
	Network,
	Timeout,
	Serialization,
}
