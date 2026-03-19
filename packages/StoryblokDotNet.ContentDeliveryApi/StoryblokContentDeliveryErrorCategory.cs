namespace StoryblokDotNet.ContentDeliveryApi;

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
