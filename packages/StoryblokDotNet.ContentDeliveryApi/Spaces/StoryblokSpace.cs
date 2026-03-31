using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class StoryblokSpace
{
	[JsonPropertyName("id")]
	public long Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("domain")]
	public string Domain { get; set; } = string.Empty;

	[JsonPropertyName("version")]
	public long Version { get; set; }

	[JsonPropertyName("language_codes")]
	public IReadOnlyList<string> LanguageCodes { get; set; } = [];
}
