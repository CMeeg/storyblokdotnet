# Caching

`StoryblokDotNet.ContentDeliveryApi` integrates with `HybridCache` and supports request-level cache entry options.

## Default behaviour

- Global cache usage is enabled by default (`UseCache = true`)
- If `HybridCache` is already registered, it is reused
- If not registered, an in-memory `HybridCache` is added
- Cache version (`cv`) is resolved automatically for non-`/spaces/me` requests

## Configure cache usage

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
    options.Cache.UseCache = true;
    options.Cache.CvTtl = 3600;
});
```

Set `UseCache = false` to fully disable library caching.

## Configure per-client cache defaults

```csharp
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Caching;

services.AddStoryblokContentDeliveryApi(options =>
{
    options.Clients.Clear();
    options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
    {
        Region = StoryblokRegion.Eu,
        Token = "EU_TOKEN",
        Cache = new StoryblokContentDeliveryApiCacheOptions
        {
            UseCache = true,
            CvTtl = 900,
        },
    });
});
```

## Opt in per request

```csharp
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

StoryblokContentDeliveryApiCacheEntryOptions cacheEntryOptions = new()
{
    Expiration = TimeSpan.FromMinutes(5),
    LocalCacheExpiration = TimeSpan.FromMinutes(1),
};
cacheEntryOptions.Tags.Add("storyblok");
cacheEntryOptions.Tags.Add("spaces");

StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
    await apiClient.Spaces().RetrieveCurrentSpace(
        cacheEntryOptions: cacheEntryOptions,
        cancellationToken: cancellationToken);
```

## Invalidation

Invalidation strategy is controlled by your app:

- Clear one request key: `Clear(request)`
- Clear by tag: `ClearByTag(tag)`
- Clear all keys: `ClearAll()`
- Clear internal cache-version entry: `ClearCvCache()`

A common pattern is calling `ClearCvCache()` from a Storyblok webhook handler.
