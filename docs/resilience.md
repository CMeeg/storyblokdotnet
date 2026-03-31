# Resilience

The library configures an HTTP resilience pipeline for Storyblok requests.

## What is enabled

By default, retry handling is enabled with:

- Exponential backoff
- Optional jitter
- Optional support for `Retry-After`
- Retry on network failures
- Retry on transient server errors
- Retry on `429 Too Many Requests`

## Configure resilience

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
    options.Resilience.Enabled = true;
    options.Resilience.MaxRetryAttempts = 4;
    options.Resilience.InitialDelay = TimeSpan.FromMilliseconds(200);
    options.Resilience.MaxDelay = TimeSpan.FromSeconds(5);
    options.Resilience.BackoffMultiplier = 2;
    options.Resilience.UseJitter = true;
    options.Resilience.RetryOnRateLimit = true;
    options.Resilience.RetryOnTransientServerErrors = true;
    options.Resilience.RespectRetryAfterHeader = true;
});
```

## Disable retries

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
    options.Resilience.Enabled = false;
});
```

Or keep resilience on but set:

```csharp
options.Resilience.MaxRetryAttempts = 0;
```

## Notes

- Cancellation initiated by your `CancellationToken` is not retried.
- Invalid resilience values fail options validation when the client is resolved.
