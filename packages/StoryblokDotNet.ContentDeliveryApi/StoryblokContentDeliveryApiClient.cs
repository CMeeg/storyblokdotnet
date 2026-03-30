using System.Collections.Concurrent;
using StoryblokDotNet.ContentDeliveryApi.Caching;
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
	private readonly StoryblokContentDeliveryCacheOptions defaultCacheOptions;
	private readonly StoryblokRegion defaultRegion;
	private readonly Func<HttpClient> httpClientFactory;
	private readonly StoryblokContentDeliveryHttpClient contentDeliveryHttpClient;
	private readonly IStoryblokContentDeliveryApiCache cache;

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
		StoryblokContentDeliveryApiOptions options,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryApiCache cache)
		: this(options, CreateHttpClientFactory(httpClientFactory), cache)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryApiOptions options,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryApiCache cache)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(cache);
		ArgumentOutOfRangeException.ThrowIfZero(options.Clients.Count, nameof(options.Clients));

		this.httpClientFactory = httpClientFactory;
		this.cache = cache;
		this.defaultCacheOptions = CloneCacheOptions(options.Cache);

		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> resolvedDefaultsByRegion = [];

		foreach (StoryblokContentDeliveryHttpClientOptions clientOptions in options.Clients)
		{
			resolvedDefaultsByRegion[clientOptions.Region] = new StoryblokContentDeliveryHttpClientOptions
			{
				Region = clientOptions.Region,
				Token = clientOptions.Token,
				Cache = CloneCacheOptions(clientOptions.Cache),
			};
		}

		this.defaultsByRegion = resolvedDefaultsByRegion;
		this.defaultRegion = options.Clients.First().Region;
		this.clientsByRegion = new ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>>();

		contentDeliveryHttpClient = Create();
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(token ?? throw new ArgumentNullException(nameof(token))), static () => new HttpClient(), cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(token ?? throw new ArgumentNullException(nameof(token))), httpClientFactory, cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		string token,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(token ?? throw new ArgumentNullException(nameof(token))), httpClientFactory, cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(client ?? throw new ArgumentNullException(nameof(client))), static () => new HttpClient(), cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		IHttpClientFactory httpClientFactory,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(client ?? throw new ArgumentNullException(nameof(client))), httpClientFactory, cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	public StoryblokContentDeliveryApiClient(
		StoryblokContentDeliveryHttpClientOptions client,
		Func<HttpClient> httpClientFactory,
		IStoryblokContentDeliveryApiCache? cache = null)
		: this(new StoryblokContentDeliveryApiOptions(client ?? throw new ArgumentNullException(nameof(client))), httpClientFactory, cache ?? StoryblokContentDeliveryNoOpApiCache.Instance)
	{
	}

	private StoryblokContentDeliveryApiClient(
		Func<HttpClient> httpClientFactory,
		Dictionary<StoryblokRegion, StoryblokContentDeliveryHttpClientOptions> defaultsByRegion,
		StoryblokContentDeliveryCacheOptions defaultCacheOptions,
		StoryblokRegion defaultRegion,
		ConcurrentDictionary<StoryblokRegion, Lazy<StoryblokContentDeliveryHttpClient>> clientsByRegion,
		IStoryblokContentDeliveryApiCache cache,
		StoryblokRegion region)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(defaultsByRegion);
		ArgumentNullException.ThrowIfNull(clientsByRegion);

		this.httpClientFactory = httpClientFactory;
		this.defaultsByRegion = defaultsByRegion;
		this.defaultCacheOptions = defaultCacheOptions;
		this.defaultRegion = defaultRegion;
		this.clientsByRegion = clientsByRegion;
		this.cache = cache;

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
				Cache = CloneCacheOptions(defaultCacheOptions),
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
			Cache = CloneCacheOptions(options.Cache),
		};

		HttpClient httpClient = httpClientFactory();
		httpClient.BaseAddress = GetBaseAddress(resolvedOptions.Region);

		return new StoryblokContentDeliveryHttpClient(httpClient, resolvedOptions, cache);
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
			defaultCacheOptions,
			defaultRegion,
			clientsByRegion,
			cache,
			region);
	}

	private static StoryblokContentDeliveryCacheOptions CloneCacheOptions(StoryblokContentDeliveryCacheOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		return new StoryblokContentDeliveryCacheOptions
		{
			UseCache = options.UseCache,
			CvTtl = options.CvTtl,
		};
	}

	public Task Clear(StoryblokContentDeliveryRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		return contentDeliveryHttpClient.Clear(request, cancellationToken);
	}

	public Task ClearByTag(string tag, CancellationToken cancellationToken = default)
	{
		return contentDeliveryHttpClient.ClearByTag(tag, cancellationToken);
	}

	public Task ClearAll(CancellationToken cancellationToken = default)
	{
		return contentDeliveryHttpClient.ClearAll(cancellationToken);
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
