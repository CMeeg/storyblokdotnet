namespace StoryblokDotNet.ContentDeliveryApi.Caching;

public sealed class StoryblokContentDeliveryApiCacheOptions
{
	public const int DefaultCvTtl = 3600;

	public bool UseCache { get; set; } = true;

	public int CvTtl { get; set; } = DefaultCvTtl;
}
