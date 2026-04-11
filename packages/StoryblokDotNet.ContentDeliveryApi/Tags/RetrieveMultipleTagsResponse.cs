using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class RetrieveMultipleTagsResponse
{
	[JsonPropertyName("tags")]
	public IReadOnlyList<StoryblokTag> Tags { get; set; } = [];
}
