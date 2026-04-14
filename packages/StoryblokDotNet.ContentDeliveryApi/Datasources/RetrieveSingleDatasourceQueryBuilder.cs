namespace StoryblokDotNet.ContentDeliveryApi.Datasources;

public sealed class RetrieveSingleDatasourceQueryBuilder : IStoryblokContentDeliveryQueryBuilder<RetrieveSingleDatasourceQuery>
{
	private readonly RetrieveSingleDatasourceQuery query = new();

	public RetrieveSingleDatasourceQueryBuilder WithToken(string token)
	{
		query.Token = token;

		return this;
	}

	public RetrieveSingleDatasourceQueryBuilder WithCv(long cv)
	{
		query.Cv = cv;

		return this;
	}

	public RetrieveSingleDatasourceQuery Build()
	{
		return new RetrieveSingleDatasourceQuery
		{
			Token = query.Token,
			Cv = query.Cv,
		};
	}
}
