using System.Collections.Concurrent;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryHttpClientFactory
{
	public const string HttpClientName = "StoryblokContentDeliveryApi";

	private static readonly Uri EuBaseAddress = new("https://api.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri UsBaseAddress = new("https://api-us.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri CanadaBaseAddress = new("https://api-ca.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri AustraliaBaseAddress = new("https://api-ap.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri ChinaBaseAddress = new("https://app.storyblokchina.cn/v2/cdn", UriKind.Absolute);

	private readonly ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion = new();
	private readonly Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> defaultsByRegion;
	private readonly StoryblokRegion defaultRegion;
	private readonly Func<HttpClient> httpClientFactory;

	internal static IReadOnlyList<StoryblokRegion> Regions { get; } =
	[
		StoryblokRegion.Eu,
		StoryblokRegion.Us,
		StoryblokRegion.Canada,
		StoryblokRegion.Australia,
		StoryblokRegion.China,
	];

	public StoryblokContentDeliveryHttpClientFactory(
		IHttpClientFactory httpClientFactory,
		StoryblokContentDeliveryApiOptions options)
		: this(() => httpClientFactory.CreateClient(HttpClientName), options)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
	}

	public StoryblokContentDeliveryHttpClientFactory(
		Func<HttpClient> httpClientFactory,
		StoryblokContentDeliveryApiOptions options)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(options);

		this.httpClientFactory = httpClientFactory;

		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> resolvedDefaultsByRegion = [];

		foreach (StoryblokContentDeliveryHttpClientOptions clientOptions in options.Clients)
		{
			resolvedDefaultsByRegion[clientOptions.Region] = new StoryblokContentDeliveryHttpClientOptions
			{
				Region = clientOptions.Region,
				Token = clientOptions.Token,
			};
		}

		this.defaultsByRegion = resolvedDefaultsByRegion;
		this.defaultRegion = options.Clients.First().Region;
	}

	public StoryblokContentDeliveryHttpClient Create(StoryblokRegion? region = null)
	{
		StoryblokRegion resolvedRegion = region ?? defaultRegion;

		StoryblokContentDeliveryHttpClientOptions options = defaultsByRegion.TryGetValue(resolvedRegion, out StoryblokContentDeliveryHttpClientOptions? configuredOptions)
			? configuredOptions
			: new StoryblokContentDeliveryHttpClientOptions
			{
				Region = resolvedRegion,
			};

		return Create(options);
	}

	private StoryblokContentDeliveryHttpClient Create(StoryblokContentDeliveryHttpClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		Lazy<StoryblokContentDeliveryHttpClient> lazyClient = clientsByRegion.GetOrAdd(
			options.Region,
			region => new Lazy<StoryblokContentDeliveryHttpClient>(
				() => CreateClient(options),
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

		HttpClient httpClient = httpClientFactory();
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
