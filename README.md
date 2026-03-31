# storyblokdotnet

`StoryblokDotNet.ContentDeliveryApi` is a .NET class library for calling Storyblok's [Content Delivery API](https://www.storyblok.com/docs/api/content-delivery/v2). It provides a typed client, built-in caching support, and resilient HTTP behaviour with minimal setup.

## Getting started

Install from NuGet:

```bash
dotnet add package StoryblokDotNet.ContentDeliveryApi
```

Configure a token and optional region (default is "Eu") in `appsettings.json`:

```json
{
  "Storyblok": {
    "ContentDelivery": {
      "Token": "YOUR_CONTENT_DELIVERY_API_TOKEN",
      "Region": "Eu"
    }
  }
}
```

Register the default client:

```csharp
using Microsoft.Extensions.DependencyInjection;
using StoryblokDotNet.ContentDeliveryApi;

// Register options
builder.Services.Configure<StoryblokContentDeliveryApiOptions>(
    builder.Configuration.GetSection("Storyblok:ContentDelivery"));

// Register the client and its dependencies
builder.Services.AddStoryblokContentDeliveryApi();
```

Inject and use the client:

```csharp
using StoryblokDotNet.ContentDeliveryApi;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokSpaceService
{
    private readonly StoryblokContentDeliveryApiClient apiClient;

    public StoryblokSpaceService(StoryblokContentDeliveryApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task<string?> GetSpaceName(CancellationToken cancellationToken = default)
    {
        StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
            await apiClient.Spaces().RetrieveCurrentSpace(cancellationToken: cancellationToken);

        return result.IsSuccess
            ? result.Data.Space.Name
            : null;
    }
}
```

## Usage

The client and feature areas map closely to Storyblok's own API documentation, so endpoint structure and behaviour should feel familiar if you already use the HTTP API.

### Endpoint map

| Area | API client entry point | Operations |
| --- | --- | --- |
| Spaces | `apiClient.Spaces()` | `RetrieveCurrentSpace()` |

### Regions and Authentication

Set `Region` and `Token` on your default client configuration.

- `Region` selects the correct Storyblok Content Delivery API base URL automatically.
- `Token` is added automatically to requests made by the client.

For multi-region and per-region token setups, see [docs/multi-region.md](docs/multi-region.md).

### Error handling

All operations return `StoryblokContentDeliveryResult<TResponse>`.

- On success, use `Data`.
- On failure, use `Error` (including `Category`, `StatusCode`, and details).
- Error categories map to Storyblok's documented [error types](https://www.storyblok.com/docs/api/content-delivery/v2#errors).

```csharp
StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
    await apiClient.Spaces().RetrieveCurrentSpace(cancellationToken: cancellationToken);

if (!result.IsSuccess)
{
    StoryblokContentDeliveryError error = result.Error!;
    logger.LogWarning("Storyblok request failed: {Category} ({StatusCode}) {Message}",
        error.Category,
        error.StatusCode,
        error.Message);
    return;
}

RetrieveCurrentSpaceResponse data = result.Data;
```

### Caching

Caching is built in and uses `HybridCache`.

- If you register your own `HybridCache` first, this library uses it.
- If no `HybridCache` is registered, an in-memory `HybridCache` is added by default.
- The Storyblok [cache version](https://www.storyblok.com/docs/concepts/caching#cache-version) (`cv`) is requested and cached automatically.

Cache invalidation is application-specific. A common pattern is a Storyblok [webhook](https://www.storyblok.com/docs/concepts/webhooks) that calls `ClearCvCache()`, or you can allow entries to expire naturally based on TTL.

```csharp
await apiClient.ClearCvCache(cancellationToken);
```

You can opt in to caching for endpoint calls using `StoryblokContentDeliveryApiCacheEntryOptions`.

```csharp
using StoryblokDotNet.ContentDeliveryApi.Caching;

StoryblokContentDeliveryApiCacheEntryOptions cacheEntryOptions = new()
{
    Expiration = TimeSpan.FromMinutes(5),
};
cacheEntryOptions.Tags.Add("spaces");

StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
    await apiClient.Spaces().RetrieveCurrentSpace(cacheEntryOptions: cacheEntryOptions, cancellationToken: cancellationToken);
```

To change `CvTtl` or disable caching (`UseCache`), see [docs/configuration.md](docs/configuration.md) and [docs/caching.md](docs/caching.md).

### Rate limits

Handling for Storyblok [rate limits](https://www.storyblok.com/docs/api/content-delivery/v2#rate-limits) is built in through the HTTP client resilience pipeline.

For detailed retry and resilience tuning, see [docs/resilience.md](docs/resilience.md).

## More information

Additional guides are available for advanced scenarios:

### Configuration

For deeper configuration via `services.AddStoryblokContentDeliveryApi(Action<StoryblokContentDeliveryApiOptions> configureOptions)`, see [docs/configuration.md](docs/configuration.md).

### Manual wiring

If you cannot use DI, or want direct construction for tests and isolated components, see [docs/manual-wiring.md](docs/manual-wiring.md).
