# Datasources

The Datasources area gives you access to individual datasource definitions returned by the Storyblok Content Delivery API.

## `RetrieveSingleDatasource()`

This maps to Storyblok's [Retrieve a Single Datasource](https://www.storyblok.com/docs/api/content-delivery/v2/datasources/retrieve-a-single-datasource) endpoint.

Returns a datasource object identified by its ID/Slug.

### Get a single datasource

This example retrieves a datasource and prints basic metadata.

```csharp
using StoryblokDotNet.ContentDeliveryApi.Datasources;

StoryblokContentDeliveryResult<RetrieveSingleDatasourceResponse> result =
    await apiClient.Datasources().RetrieveSingleDatasource("sizes", cancellationToken: cancellationToken);

if (!result.IsSuccess)
{
    Console.WriteLine(result.Error.Message);
    return;
}

Console.WriteLine(result.Data.Datasource.Name);
```
