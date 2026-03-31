using Microsoft.Extensions.DependencyInjection;
using System.Net;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Http;

public sealed class StoryblokContentDeliveryApiResilienceTests
{
	[Fact]
	public void AddStoryblokContentDeliveryApiResilience_WithManualHttpClientBuilder_RegistersNamedClient()
	{
		ServiceCollection services = new();
		services
			.AddHttpClient("manual-storyblok")
			.AddStoryblokContentDeliveryApiResilience(new StoryblokContentDeliveryApiResilienceOptions
			{
				MaxRetryAttempts = 1,
				UseJitter = false,
			});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

		HttpClient client = httpClientFactory.CreateClient("manual-storyblok");

		Assert.NotNull(client);
	}

	[Fact]
	public void ResolveRetryDelay_WithFirstRetryWithoutRetryAfter_UsesInitialDelay()
	{
		StoryblokContentDeliveryApiResilienceOptions resilienceOptions = new()
		{
			InitialDelay = TimeSpan.FromMilliseconds(250),
			UseJitter = false,
		};

		TimeSpan retryDelay = StoryblokContentDeliveryApiResilience.ResolveRetryDelay(0, null, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(250), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectEnabled_UsesRetryAfterValue()
	{
		StoryblokContentDeliveryApiResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = true,
			MaxDelay = TimeSpan.FromSeconds(10),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryApiResilience.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromSeconds(3), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectDisabled_UsesBackoffDelay()
	{
		StoryblokContentDeliveryApiResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = false,
			InitialDelay = TimeSpan.FromMilliseconds(400),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryApiResilience.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(400), retryDelay);
	}
}
