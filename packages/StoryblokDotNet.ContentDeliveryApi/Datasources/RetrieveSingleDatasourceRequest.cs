namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class RetrieveSingleDatasourceRequest : StoryblokContentDeliveryRequest
{
	internal const string RetrieveSingleDatasourcePath = "/datasources";

	public RetrieveSingleDatasourceRequest(string datasourceId, RetrieveSingleDatasourceQuery query)
		: base($"{RetrieveSingleDatasourcePath}/{EscapeDatasourceId(datasourceId)}", query)
	{
	}

	private static string EscapeDatasourceId(string datasourceId)
	{
		if (string.IsNullOrWhiteSpace(datasourceId))
		{
			throw new ArgumentException("Datasource id cannot be null, empty, or whitespace.", nameof(datasourceId));
		}

		return Uri.EscapeDataString(datasourceId);
	}
}
