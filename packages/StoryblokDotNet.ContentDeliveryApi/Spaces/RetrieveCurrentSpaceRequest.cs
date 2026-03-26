namespace StoryblokDotNet.ContentDeliveryApi.Spaces;

public sealed class RetrieveCurrentSpaceRequest : StoryblokContentDeliveryRequest
{
	internal const string RetrieveCurrentSpacePath = "/spaces/me";

	public RetrieveCurrentSpaceRequest(RetrieveCurrentSpaceQuery query)
		: base(RetrieveCurrentSpacePath, query)
	{
	}
}
