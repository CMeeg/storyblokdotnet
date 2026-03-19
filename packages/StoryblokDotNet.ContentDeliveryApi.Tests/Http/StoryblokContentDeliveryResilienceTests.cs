using Microsoft.Extensions.DependencyInjection;
using System.Net;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Http;

public sealed class StoryblokContentDeliveryResilienceTests
{
	[Fact]
	public void AddStoryblokContentDeliveryResilience_WithManualHttpClientBuilder_RegistersNamedClient()
	{
		ServiceCollection services = new();
		services
			.AddHttpClient("manual-storyblok")
			.AddStoryblokContentDeliveryResilience(new StoryblokContentDeliveryResilienceOptions
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
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			InitialDelay = TimeSpan.FromMilliseconds(250),
			UseJitter = false,
		};

		TimeSpan retryDelay = StoryblokContentDeliveryResilience.ResolveRetryDelay(0, null, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(250), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectEnabled_UsesRetryAfterValue()
	{
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = true,
			MaxDelay = TimeSpan.FromSeconds(10),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryResilience.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromSeconds(3), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectDisabled_UsesBackoffDelay()
	{
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = false,
			InitialDelay = TimeSpan.FromMilliseconds(400),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryResilience.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(400), retryDelay);
	}
}
