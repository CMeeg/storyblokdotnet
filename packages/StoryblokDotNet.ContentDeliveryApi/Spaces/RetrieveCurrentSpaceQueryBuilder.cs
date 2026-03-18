namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class RetrieveCurrentSpaceQueryBuilder : IStoryblokContentDeliveryQueryBuilder<RetrieveCurrentSpaceQuery>
{
	private readonly RetrieveCurrentSpaceQuery query = new();

	public RetrieveCurrentSpaceQueryBuilder WithToken(string token)
	{
		query.Token = token;

		return this;
	}

	public RetrieveCurrentSpaceQuery Build()
	{
		return new RetrieveCurrentSpaceQuery
		{
			Token = query.Token,
		};
	}
}
