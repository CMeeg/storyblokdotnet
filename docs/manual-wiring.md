# Manual wiring

This guide shows how to construct `StoryblokContentDeliveryApiClient` without dependency injection.

## When to use this

- Unit or integration test setups
- Console tools with minimal infrastructure
- Components where DI is unavailable

## Simplest constructor

```csharp
using StoryblokDotNet.ContentDeliveryApi;

StoryblokContentDeliveryApiClient apiClient = new("YOUR_CONTENT_DELIVERY_API_TOKEN");
```

This uses:

- `StoryblokRegion.Eu`
- A default `HttpClient`
- A no-op cache implementation

## Constructor with client options

```csharp
using StoryblokDotNet.ContentDeliveryApi;
using StoryblokDotNet.ContentDeliveryApi.Http;

StoryblokContentDeliveryApiHttpClientOptions clientOptions = new()
{
    Region = StoryblokRegion.Us,
    Token = "YOUR_CONTENT_DELIVERY_API_TOKEN",
};

StoryblokContentDeliveryApiClient apiClient = new(clientOptions);
```

## Constructor with custom cache

```csharp
using StoryblokDotNet.ContentDeliveryApi;
using StoryblokDotNet.ContentDeliveryApi.Caching;

IStoryblokContentDeliveryApiCache cache = new MyStoryblokCache();
StoryblokContentDeliveryApiClient apiClient = new("YOUR_CONTENT_DELIVERY_API_TOKEN", cache);
```

`MyStoryblokCache` is your own implementation of `IStoryblokContentDeliveryApiCache`.

In most applications, DI-based registration is still recommended because it sets up:

- `IHttpClientFactory`
- Resilience pipeline
- `HybridCache` integration
- Region-specific client access via `ForRegion(...)`

## Example call

```csharp
using StoryblokDotNet.ContentDeliveryApi.Spaces;

StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
    await apiClient.Spaces().RetrieveCurrentSpace(cancellationToken: cancellationToken);

if (result.IsSuccess)
{
    Console.WriteLine(result.Data.Space.Name);
}
else
{
    Console.WriteLine(result.Error?.Message);
}
```
