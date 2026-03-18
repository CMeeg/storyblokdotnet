namespace StoryblokDotNet.ContentDeliveryApi;

public interface IStoryblokContentDeliveryQueryBuilder<out TQuery>
	where TQuery : StoryblokContentDeliveryQuery
{
	TQuery Build();
}
