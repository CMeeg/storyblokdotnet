using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class StoryblokTag
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("taggings_count")]
	public int TaggingsCount { get; set; }
}
