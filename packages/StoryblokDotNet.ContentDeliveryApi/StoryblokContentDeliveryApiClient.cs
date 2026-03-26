using System.Collections.Concurrent;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiClient
{
	public const string HttpClientName = "StoryblokContentDeliveryApi";

	private static readonly Uri EuBaseAddress = new("https://api.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri UsBaseAddress = new("https://api-us.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri CanadaBaseAddress = new("https://api-ca.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri AustraliaBaseAddress = new("https://api-ap.storyblok.com/v2/cdn", UriKind.Absolute);
	private static readonly Uri ChinaBaseAddress = new("https://app.storyblokchina.cn/v2/cdn", UriKind.Absolute);

	private readonly ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion;
	private readonly Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> defaultsByRegion;
	private readonly StoryblokRegion defaultRegion;
	private readonly Func<HttpClient> httpClientFactory;
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;
	private readonly IStoryblokContentDeliveryCvCache cvCache;

	internal static IReadOnlyList<StoryblokRegion> Regions { get; } =
	[
		StoryblokRegion.Eu,
		StoryblokRegion.Us,
		StoryblokRegion.Canada,
		StoryblokRegion.Australia,
		StoryblokRegion.China,
	];

	public StoryblokRegion Region => contentDeliveryHttpClient.Options.Region;
	public string Token => contentDeliveryHttpClient.Options.Token;

	public StoryblokContentDeliveryApiClient(
		IList<StoryblokContentDeliveryHttpClientOptions> clients,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryCvCache cvCache)
		: this(clients, CreateHttpClientFactory(httpClientFactory), cvCache)
	{
	}

	public StoryblokContentDeliveryApiClient(
		IList<StoryblokContentDeliveryHttpClientOptions> clients,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryCvCache cvCache)
	{
		ArgumentNullException.ThrowIfNull(clients);
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(cvCache);
		ArgumentOutOfRangeException.ThrowIfZero(clients.Count, nameof(clients));

		this.httpClientFactory = httpClientFactory;
		this.cvCache = cvCache;

		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> resolvedDefaultsByRegion = [];

		foreach (StoryblokContentDeliveryHttpClientOptions clientOptions in clients)
		{
			resolvedDefaultsByRegion[clientOptions.Region] = new StoryblokContentDeliveryHttpClientOptions
			{
				Region = clientOptions.Region,
				Token = clientOptions.Token,
			};
		}

		this.defaultsByRegion = resolvedDefaultsByRegion;
		this.defaultRegion = clients.First().Region;
		this.clientsByRegion = new ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>>();

		contentDeliveryHttpClient = Create();
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([new StoryblokContentDeliveryHttpClientOptions { Token = token ?? throw new ArgumentNullException(nameof(token)) }], static () => new HttpClient(), cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([new StoryblokContentDeliveryHttpClientOptions { Token = token ?? throw new ArgumentNullException(nameof(token)) }], httpClientFactory, cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([new StoryblokContentDeliveryHttpClientOptions { Token = token ?? throw new ArgumentNullException(nameof(token)) }], httpClientFactory, cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([client ?? throw new ArgumentNullException(nameof(client))], static () => new HttpClient(), cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([client ?? throw new ArgumentNullException(nameof(client))], httpClientFactory, cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryCvCache? cvCache = null)
		: this([client ?? throw new ArgumentNullException(nameof(client))], httpClientFactory, cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance)
	{
	}

	private StoryblokContentDeliveryApiClient(
		Func<HttpClient> httpClientFactory,
		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> defaultsByRegion,
		StoryblokRegion defaultRegion,
		ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion,
		IStoryblokContentDeliveryCvCache cvCache,
		StoryblokRegion region)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(defaultsByRegion);
		ArgumentNullException.ThrowIfNull(clientsByRegion);

		this.httpClientFactory = httpClientFactory;
		this.defaultsByRegion = defaultsByRegion;
		this.defaultRegion = defaultRegion;
		this.clientsByRegion = clientsByRegion;
		this.cvCache = cvCache;

		contentDeliveryHttpClient = Create(region);
	}

	private static Func<HttpClient> CreateHttpClientFactory(IHttpClientFactory httpClientFactory)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		return () => httpClientFactory.CreateClient(HttpClientName);
	}

	private StoryblokContentDeliveryHttpClient Create(StoryblokRegion? region = null)
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
			_ => new Lazy<StoryblokContentDeliveryHttpClient>(
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

		return new StoryblokContentDeliveryHttpClient(httpClient, resolvedOptions, cvCache);
	}

	public StoryblokContentDeliveryApiClient ForRegion(StoryblokRegion region)
	{
		if (region == Region)
		{
			return this;
		}

		return new StoryblokContentDeliveryApiClient(
			httpClientFactory,
			defaultsByRegion,
			defaultRegion,
			clientsByRegion,
			cvCache,
			region);
	}

	public Task ClearCv(CancellationToken cancellationToken = default)
	{
		return cvCache.ClearCv(Region, cancellationToken);
	}

	public StoryblokContentDeliverySpacesApi Spaces()
	{
		return new StoryblokContentDeliverySpacesApi(contentDeliveryHttpClient);
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
