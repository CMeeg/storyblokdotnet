using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class StoryblokDatasource
{
	[JsonPropertyName("id")]
	public long Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("slug")]
	public string Slug { get; set; } = string.Empty;

	[JsonPropertyName("dimensions")]
	public IReadOnlyList<StoryblokDatasourceDimension> Dimensions { get; set; } = [];
}
