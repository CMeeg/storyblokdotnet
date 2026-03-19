using System.Net;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryResilienceOptions
{
	public bool Enabled { get; set; } = true;

	public int MaxRetryAttempts { get; set; } = 3;

	public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(200);

	public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(5);

	public double BackoffMultiplier { get; set; } = 2;

	public bool UseJitter { get; set; } = true;

	public bool RetryOnRateLimit { get; set; } = true;

	public bool RetryOnTransientServerErrors { get; set; } = true;

	public bool RespectRetryAfterHeader { get; set; } = true;

	internal bool ShouldRetryStatusCode(HttpStatusCode statusCode)
	{
		if (RetryOnRateLimit && statusCode == HttpStatusCode.TooManyRequests)
		{
			return true;
		}

		if (!RetryOnTransientServerErrors)
		{
			return false;
		}

		return statusCode == HttpStatusCode.InternalServerError
			|| statusCode == HttpStatusCode.BadGateway
			|| statusCode == HttpStatusCode.ServiceUnavailable
			|| statusCode == HttpStatusCode.GatewayTimeout;
	}
}
