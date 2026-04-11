# Tags

The Tags area gives you access to the tag list returned by the Storyblok Content Delivery API.

## `RetrieveMultipleTags()`

This maps to Storyblok's [Retrieve Multiple Tags](https://www.storyblok.com/docs/api/content-delivery/v2/tags/retrieve-multiple-tags) endpoint.

Returns an array of tag objects used in a space. Only tags assigned to at least one story are included.

### Get all tags

This example retrieves the available tags for the configured space.

```csharp
using StoryblokDotNet.ContentDeliveryApi.Tags;

StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse> result =
    await apiClient.Tags().RetrieveMultipleTags(cancellationToken: cancellationToken);

if (!result.IsSuccess)
{
    Console.WriteLine(result.Error.Message);
    return;
}

foreach (StoryblokTag tag in result.Data.Tags)
{
    Console.WriteLine($"{tag.Name}: {tag.TaggingsCount}");
}
```

### Filter tags by folder prefix and version

Use `StartsWith` to limit tags to a content path and `Version` to choose draft or published content.

If you do not set `Version`, Storyblok defaults to `published`.

```csharp
using StoryblokDotNet.ContentDeliveryApi.Tags;

StoryblokContentDeliveryResult<RetrieveMultipleTagsResponse> result =
    await apiClient.Tags().RetrieveMultipleTags(
        query => query
            .WithStartsWith("blog/")
            .WithVersion(StoryblokVersion.Draft),
        cancellationToken: cancellationToken);
```
