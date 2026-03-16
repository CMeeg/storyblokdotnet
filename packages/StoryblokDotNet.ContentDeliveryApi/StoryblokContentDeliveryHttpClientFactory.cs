using System.Collections.Concurrent;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryHttpClientFactory
{
	private const string RegionClientNamePrefix = "StoryblokContentDeliveryApi";

	private static readonly Uri EuBaseAddress = new("https://api.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri UsBaseAddress = new("https://api-us.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri CanadaBaseAddress = new("https://api-ca.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri AustraliaBaseAddress = new("https://api-ap.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri ChinaBaseAddress = new("https://app.storyblokchina.cn/v2/cdn", UriKind.Absolute);

	private readonly ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion = new();
	private readonly IHttpClientFactory httpClientFactory;

	internal static IReadOnlyList<StoryblokRegion> Regions { get; } =
	[
		StoryblokRegion.Eu,
		StoryblokRegion.Us,
		StoryblokRegion.Canada,
		StoryblokRegion.Australia,
		StoryblokRegion.China,
	];

	public StoryblokContentDeliveryHttpClientFactory(IHttpClientFactory httpClientFactory)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);

		this.httpClientFactory = httpClientFactory;
	}

	public StoryblokContentDeliveryHttpClient Create(StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		Lazy<StoryblokContentDeliveryHttpClient> lazyClient = clientsByRegion.GetOrAdd(
			resolvedOptions.Region,
			region => new Lazy<StoryblokContentDeliveryHttpClient>(
				() => CreateClient(region),
				LazyThreadSafetyMode.ExecutionAndPublication));

		return lazyClient.Value;
	}

	private StoryblokContentDeliveryHttpClient CreateClient(StoryblokRegion region)
	{
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Region = region,
		};

		HttpClient httpClient = httpClientFactory.CreateClient(GetClientName(region));
		httpClient.BaseAddress ??= GetBaseAddress(region);

		return new StoryblokContentDeliveryHttpClient(httpClient, options);
	}

	internal static string GetClientName(StoryblokRegion region)
	{
		return $"{RegionClientNamePrefix}-{region}";
	}

	internal static Uri GetBaseAddress(StoryblokRegion region) => region switch
	{
		StoryblokRegion.Eu => EuBaseAddress,
		StoryblokRegion.Us => UsBaseAddress,
		StoryblokRegion.Canada => CanadaBaseAddress,
		StoryblokRegion.Australia => AustraliaBaseAddress,
		StoryblokRegion.China => ChinaBaseAddress,
		_ => throw new ArgumentOutOfRangeException(nameof(region), region, null),
	};
}
