# storyblokdotnet

`storyblokdotnet` provides a .NET client for the Storyblok Content Delivery API with region-aware HTTP clients and DI registration helpers.

## Dependency Injection

Register the API client using `IServiceCollection` extension methods.

```csharp
using StoryblokDotNet.ContentDeliveryApi;

IServiceCollection services = new ServiceCollection();

services.AddStoryblokContentDeliveryApi();
```

This registers:

- default `StoryblokContentDeliveryApiClient` (unkeyed)
- keyed `StoryblokContentDeliveryApiClient` registrations per configured region

## Configure A Single Default Region

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
	options.Region = StoryblokRegion.Us;
});
```

## Configure One Or More Clients Via Options Pattern

Use `StoryblokContentDeliveryApiOptions` to configure multiple client entries.

```csharp
services.AddStoryblokContentDeliveryApi(options =>
{
	options.Clients.Clear();

	options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
	{
		Region = StoryblokRegion.Canada,
	});

	options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
	{
		Region = StoryblokRegion.Australia,
	});
});
```

Behavior:

- the unkeyed `StoryblokContentDeliveryApiClient` uses the first configured client
- keyed clients require that region to be configured in `Clients`

Validation rules:

- `Clients` must contain at least one entry
- each entry must use a valid `StoryblokRegion`
- at most one configuration can be supplied per `StoryblokRegion`

## Configure From IConfiguration

### Multi-client configuration

```json
{
	"Storyblok": {
		"ContentDelivery": {
			"Clients": [
				{
					"Region": "Eu"
				},
				{
					"Region": "Us"
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

### Legacy single-client configuration

The single-region shape is still supported:

```json
{
	"Storyblok": {
		"ContentDelivery": {
			"Region": "Canada"
		}
	}
}
```

When this shape is used, it is treated as a single configured client.

## Resolve Keyed Clients By Region

```csharp
using Microsoft.Extensions.DependencyInjection;
using StoryblokDotNet.ContentDeliveryApi;

using ServiceProvider serviceProvider = services.BuildServiceProvider();

StoryblokContentDeliveryApiClient euClient =
	serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Eu);

StoryblokContentDeliveryApiClient usClient =
	serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us);
```

Notes:

- keyed resolution only succeeds for regions configured in `StoryblokContentDeliveryApiOptions.Clients`
- resolving an unconfigured region throws `InvalidOperationException`

## End-To-End Sample

```csharp
using Microsoft.Extensions.DependencyInjection;
using StoryblokDotNet.ContentDeliveryApi;

ServiceCollection services = new();

services.AddStoryblokContentDeliveryApi(options =>
{
	options.Clients.Clear();
	options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
	{
		Region = StoryblokRegion.Canada,
	});
	options.Clients.Add(new StoryblokContentDeliveryApiHttpClientOptions
	{
		Region = StoryblokRegion.Us,
	});
});

using ServiceProvider serviceProvider = services.BuildServiceProvider();

// Default client uses the first configured entry (Canada).
StoryblokContentDeliveryApiClient defaultClient =
	serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

// Keyed clients map to explicitly configured regions.
StoryblokContentDeliveryApiClient usClient =
	serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us);

Console.WriteLine(defaultClient.ContentDeliveryHttpClient.Options.Region); // Canada
Console.WriteLine(usClient.ContentDeliveryHttpClient.Options.Region);      // Us
```

Unconfigured keyed region example:

```csharp
InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
	() => serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Eu));

Console.WriteLine(exception.Message);
// No Storyblok client configuration was supplied for region 'Eu'.
```

## End-To-End IConfiguration Sample

`appsettings.json`:

```json
{
	"Storyblok": {
		"ContentDelivery": {
			"Clients": [
				{
					"Region": "Canada"
				},
				{
					"Region": "Us"
				}
			]
		}
	}
}
```

Program setup and resolution:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoryblokDotNet.ContentDeliveryApi;

ServiceCollection services = new();

IConfiguration configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false)
	.Build();

services.AddStoryblokContentDeliveryApi(
	configuration.GetSection("Storyblok:ContentDelivery"));

using ServiceProvider serviceProvider = services.BuildServiceProvider();

StoryblokContentDeliveryApiClient defaultClient =
	serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();
StoryblokContentDeliveryApiClient usClient =
	serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us);

Console.WriteLine(defaultClient.ContentDeliveryHttpClient.Options.Region); // Canada
Console.WriteLine(usClient.ContentDeliveryHttpClient.Options.Region);      // Us
```
