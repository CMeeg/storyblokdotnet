using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class StoryblokDatasourceDimension
{
	[JsonPropertyName("id")]
	public long Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("entry_value")]
	public string EntryValue { get; set; } = string.Empty;

	[JsonPropertyName("datasource_id")]
	public long DatasourceId { get; set; }

	[JsonPropertyName("created_at")]
	public string CreatedAt { get; set; } = string.Empty;

	[JsonPropertyName("updated_at")]
	public string UpdatedAt { get; set; } = string.Empty;
}
