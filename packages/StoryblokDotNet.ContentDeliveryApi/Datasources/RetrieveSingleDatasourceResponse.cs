using System.Text.Json.Serialization;

namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class RetrieveSingleDatasourceResponse
{
	[JsonPropertyName("datasource")]
	public StoryblokDatasource Datasource { get; set; } = new();

	[JsonPropertyName("cv")]
	public long Cv { get; set; }
}
