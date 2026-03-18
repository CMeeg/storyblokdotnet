using System.Collections.Concurrent;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryHttpClientFactory
{
	private static readonly Uri EuBaseAddress = new("https://api.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri UsBaseAddress = new("https://api-us.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri CanadaBaseAddress = new("https://api-ca.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri AustraliaBaseAddress = new("https://api-ap.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri ChinaBaseAddress = new("https://app.storyblokchina.cn/v2/cdn", UriKind.Absolute);

	private readonly ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion = new();
	private readonly Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> defaultsByRegion;
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
		: this(httpClientFactory, null as StoryblokContentDeliveryApiOptions)
	{
	}

	public StoryblokContentDeliveryHttpClientFactory(
		IHttpClientFactory httpClientFactory,
		StoryblokContentDeliveryHttpClientOptions? options)
		: this(httpClientFactory, new StoryblokContentDeliveryApiOptions(options ?? new StoryblokContentDeliveryHttpClientOptions()))
	{
	}

	public StoryblokContentDeliveryHttpClientFactory(
		IHttpClientFactory httpClientFactory,
		StoryblokContentDeliveryApiOptions? options)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);

		this.httpClientFactory = httpClientFactory;

		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> resolvedDefaultsByRegion = [];
		StoryblokContentDeliveryApiOptions resolvedOptions = options ?? new StoryblokContentDeliveryApiOptions();

		foreach (StoryblokContentDeliveryHttpClientOptions clientOptions in resolvedOptions.Clients)
		{
			resolvedDefaultsByRegion[clientOptions.Region] = new StoryblokContentDeliveryHttpClientOptions
			{
				Region = clientOptions.Region,
				Token = clientOptions.Token,
			};
		}

		this.defaultsByRegion = resolvedDefaultsByRegion;
	}

	public StoryblokContentDeliveryHttpClient Create(StoryblokRegion region)
	{
		StoryblokContentDeliveryHttpClientOptions options = defaultsByRegion.TryGetValue(region, out StoryblokContentDeliveryHttpClientOptions? configuredOptions)
			? configuredOptions
			: new StoryblokContentDeliveryHttpClientOptions
			{
				Region = region,
			};

		return Create(options);
	}

	public StoryblokContentDeliveryHttpClient Create(StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		Lazy<StoryblokContentDeliveryHttpClient> lazyClient = clientsByRegion.GetOrAdd(
			resolvedOptions.Region,
			region => new Lazy<StoryblokContentDeliveryHttpClient>(
				() => CreateClient(resolvedOptions),
				LazyThreadSafetyMode.ExecutionAndPublication));

		return lazyClient.Value;
	}

	private StoryblokContentDeliveryHttpClient CreateClient(StoryblokContentDeliveryHttpClientOptions options)
	{
		StoryblokContentDeliveryHttpClientOptions resolvedOptions = new()
		{
			Region = options.Region,
			Token = options.Token,
		};

		HttpClient httpClient = httpClientFactory.CreateClient();
		httpClient.BaseAddress = GetBaseAddress(resolvedOptions.Region);

		return new StoryblokContentDeliveryHttpClient(httpClient, resolvedOptions);
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
