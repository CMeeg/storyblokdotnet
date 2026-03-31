# Multi-region clients

Use multiple client configurations when you need different region and token combinations.

## Configuration

```json
{
  "Storyblok": {
    "ContentDelivery": {
      "Clients": [
        {
          "Region": "Eu",
          "Token": "EU_TOKEN"
        },
        {
          "Region": "Us",
          "Token": "US_TOKEN"
        }
      ]
    }
  }
}
```

```csharp
services.AddStoryblokContentDeliveryApi(
    configuration.GetSection("Storyblok:ContentDelivery"));
```

## Access a specific region

```csharp
StoryblokContentDeliveryApiClient usClient = defaultClient.ForRegion(StoryblokRegion.Us);
```

Use `ForRegion(...)` on an existing `StoryblokContentDeliveryApiClient` to access a region-specific client.

## Default client behaviour

The default `StoryblokContentDeliveryApiClient` uses the first configured entry in `Clients`.

If no token is configured for that region, requests still run but without a token unless supplied explicitly on the request query.
