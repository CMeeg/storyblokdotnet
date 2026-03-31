# Configuration

This guide covers non-default configuration using:

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
    // configure options
});
```

## Default options

If you call `AddStoryblokContentDeliveryApi()` without arguments:

- `Region` defaults to `StoryblokRegion.Eu`
- `Token` defaults to an empty string
  - You will need to provide an authentication token with the parameters of each request made via the API client
- Caching is enabled (`UseCache = true`)
- `CvTtl` defaults to `3600` seconds
- Resilience is enabled with retry behaviour

## Configure with options delegate

```csharp
using StoryblokDotNet.ContentDeliveryApi;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

services.AddStoryblokContentDeliveryApi(options =>
{
    options.Clients.Clear();
    options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
    {
        Region = StoryblokRegion.Us,
        Token = configuration["Storyblok:Token"] ?? string.Empty,
        Cache = new StoryblokContentDeliveryApiCacheOptions
        {
            UseCache = true,
            CvTtl = 1800,
        },
    });

    options.Cache.UseCache = true;
    options.Cache.CvTtl = 1800;

    options.Resilience.MaxRetryAttempts = 5;
    options.Resilience.InitialDelay = TimeSpan.FromMilliseconds(250);
    options.Resilience.MaxDelay = TimeSpan.FromSeconds(10);
    options.Resilience.BackoffMultiplier = 2;
    options.Resilience.UseJitter = true;
    options.Resilience.RespectRetryAfterHeader = true;
});
```

## Configure from IConfiguration

### Single client shape

```json
{
  "Storyblok": {
    "ContentDelivery": {
      "Region": "Eu",
      "Token": "YOUR_TOKEN",
      "Cache": {
        "UseCache": true,
        "CvTtl": 3600
      },
      "Resilience": {
        "MaxRetryAttempts": 3,
        "RespectRetryAfterHeader": true
      }
    }
  }
}
```

### Multi-client shape

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
      ],
      "Cache": {
        "UseCache": true,
        "CvTtl": 3600
      },
      "Resilience": {
        "MaxRetryAttempts": 3
      }
    }
  }
}
```

```csharp
services.AddStoryblokContentDeliveryApi(
    configuration.GetSection("Storyblok:ContentDelivery"));
```

## Validation rules

At startup, options are validated. Invalid options throw `OptionsValidationException` when resolving the client.

- `Clients` must contain at least one entry
- Regions must be valid `StoryblokRegion` values
  - Eu
  - Us
  - Canada
  - Australia
  - China
- Only one client per region is allowed
- Resilience values such as retry count and delays must be valid

## Related guides

- [caching.md](caching.md)
- [resilience.md](resilience.md)
- [multi-region.md](multi-region.md)
