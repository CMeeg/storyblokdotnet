using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public static class StoryblokContentDeliveryApiResilience
{
	public static IHttpClientBuilder AddStoryblokContentDeliveryApiResilience(
		this IHttpClientBuilder httpClientBuilder,
		StoryblokContentDeliveryApiResilienceOptions? resilienceOptions = null)
	{
		ArgumentNullException.ThrowIfNull(httpClientBuilder);

		StoryblokContentDeliveryApiResilienceOptions resolvedResilienceOptions = resilienceOptions ?? new StoryblokContentDeliveryApiResilienceOptions();

		httpClientBuilder.AddResilienceHandler("StoryblokRetry", (builder, context) =>
		{
			StoryblokContentDeliveryApiResilienceOptions options = context.ServiceProvider
				.GetService<IOptions<StoryblokContentDeliveryApiOptions>>()?.Value.Resilience
				?? resolvedResilienceOptions;

			if (!options.Enabled || options.MaxRetryAttempts == 0)
			{
				return;
			}

			builder.AddRetry(CreateRetryStrategyOptions(options));
		});

		return httpClientBuilder;
	}

	private static HttpRetryStrategyOptions CreateRetryStrategyOptions(StoryblokContentDeliveryApiResilienceOptions resilienceOptions)
	{
		return new HttpRetryStrategyOptions
		{
			MaxRetryAttempts = resilienceOptions.MaxRetryAttempts,
			ShouldHandle = args =>
			{
				if (args.Outcome.Exception is HttpRequestException)
				{
					return PredicateResult.True();
				}

				if (args.Outcome.Exception is OperationCanceledException
					&& !args.Context.CancellationToken.IsCancellationRequested)
				{
					return PredicateResult.True();
				}

				if (args.Outcome.Result is HttpResponseMessage response)
				{
					return resilienceOptions.ShouldRetryStatusCode(response.StatusCode)
						? PredicateResult.True()
						: PredicateResult.False();
				}

				return PredicateResult.False();
			},
			DelayGenerator = args =>
			{
				HttpResponseMessage? response = args.Outcome.Result;
				TimeSpan resolvedDelay = ResolveRetryDelay(args.AttemptNumber, response, resilienceOptions);
				return new ValueTask<TimeSpan?>(resolvedDelay);
			},
		};
	}

	internal static TimeSpan ResolveRetryDelay(int retryAttemptNumber, HttpResponseMessage? response, StoryblokContentDeliveryApiResilienceOptions resilienceOptions)
	{
		if (resilienceOptions.RespectRetryAfterHeader
			&& response is not null
			&& TryGetRetryAfterDelay(response, out TimeSpan retryAfterDelay))
		{
			return retryAfterDelay > resilienceOptions.MaxDelay
				? resilienceOptions.MaxDelay
				: retryAfterDelay;
		}

		int oneBasedAttemptNumber = Math.Max(1, retryAttemptNumber + 1);
		return ComputeRetryDelay(oneBasedAttemptNumber, resilienceOptions);
	}

	private static bool TryGetRetryAfterDelay(HttpResponseMessage response, out TimeSpan retryAfterDelay)
	{
		retryAfterDelay = default;

		if (response.Headers.RetryAfter is null)
		{
			return false;
		}

		if (response.Headers.RetryAfter.Delta is TimeSpan delta && delta > TimeSpan.Zero)
		{
			retryAfterDelay = delta;
			return true;
		}

		if (response.Headers.RetryAfter.Date is DateTimeOffset retryAfterDate)
		{
			TimeSpan computedDelay = retryAfterDate - DateTimeOffset.UtcNow;
			if (computedDelay > TimeSpan.Zero)
			{
				retryAfterDelay = computedDelay;
				return true;
			}
		}

		return false;
	}

	private static TimeSpan ComputeRetryDelay(int attempt, StoryblokContentDeliveryApiResilienceOptions resilienceOptions)
	{
		double exponentialMs = resilienceOptions.InitialDelay.TotalMilliseconds
			* Math.Pow(resilienceOptions.BackoffMultiplier, attempt - 1);
		double cappedMs = Math.Min(exponentialMs, resilienceOptions.MaxDelay.TotalMilliseconds);

		if (!resilienceOptions.UseJitter)
		{
			return TimeSpan.FromMilliseconds(cappedMs);
		}

		// Jitter does not require cryptographic randomness; used only to spread retry attempts.
		#pragma warning disable CA5394
		double jitterFactor = 0.5 + Random.Shared.NextDouble();
		#pragma warning restore CA5394
		double jitteredMs = Math.Min(cappedMs * jitterFactor, resilienceOptions.MaxDelay.TotalMilliseconds);

		return TimeSpan.FromMilliseconds(jitteredMs);
	}
}
