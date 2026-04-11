namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class RetrieveMultipleTagsQueryBuilder : IStoryblokContentDeliveryQueryBuilder<RetrieveMultipleTagsQuery>
{
	private readonly RetrieveMultipleTagsQuery query = new();

	public RetrieveMultipleTagsQueryBuilder WithToken(string token)
	{
		query.Token = token;

		return this;
	}

	public RetrieveMultipleTagsQueryBuilder WithCv(long cv)
	{
		query.Cv = cv;

		return this;
	}

	public RetrieveMultipleTagsQueryBuilder WithStartsWith(string startsWith)
	{
		query.StartsWith = startsWith;

		return this;
	}

	public RetrieveMultipleTagsQueryBuilder WithVersion(StoryblokVersion version)
	{
		query.Version = version;

		return this;
	}

	public RetrieveMultipleTagsQuery Build()
	{
		return new RetrieveMultipleTagsQuery
		{
			Token = query.Token,
			Cv = query.Cv,
			StartsWith = query.StartsWith,
			Version = query.Version,
		};
	}
}
