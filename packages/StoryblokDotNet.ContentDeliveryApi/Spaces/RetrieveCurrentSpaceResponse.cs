using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class RetrieveCurrentSpaceResponse
{
	[JsonPropertyName("space")]
	public StoryblokSpace Space { get; set; } = new();
}
