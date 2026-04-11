namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class RetrieveMultipleTagsRequest : StoryblokContentDeliveryRequest
{
	internal const string RetrieveMultipleTagsPath = "/tags";

	public RetrieveMultipleTagsRequest(RetrieveMultipleTagsQuery query)
		: base(RetrieveMultipleTagsPath, query)
	{
	}
}
