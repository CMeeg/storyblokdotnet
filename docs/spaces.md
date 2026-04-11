# Spaces

The Spaces area gives you access to the current space metadata exposed by the Storyblok Content Delivery API.

## `RetrieveCurrentSpace()`

This maps to Storyblok's [Retrieve Current Space](https://www.storyblok.com/docs/api/content-delivery/v2/spaces/retrieve-current-space) endpoint.

Returns the space object for the space associated with the provided access token.

### Get the current space

This is the simplest way to fetch the current space details.

```csharp
using StoryblokDotNet.ContentDeliveryApi.Spaces;

StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> result =
    await apiClient.Spaces().RetrieveCurrentSpace(cancellationToken: cancellationToken);

if (!result.IsSuccess)
{
    Console.WriteLine(result.Error.Message);
    return;
}

StoryblokSpace space = result.Data.Space;
Console.WriteLine(space.Name);
Console.WriteLine(space.Domain);
```
