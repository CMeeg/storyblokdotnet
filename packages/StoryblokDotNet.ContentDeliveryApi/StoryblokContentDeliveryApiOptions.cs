using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiOptions
{
	public IList<StoryblokContentDeliveryApiHttpClientOptions> Clients { get; }

	public StoryblokContentDeliveryApiResilienceOptions Resilience { get; }

	public StoryblokContentDeliveryApiCacheOptions Cache { get; }

	public StoryblokContentDeliveryApiOptions()
		: this(new StoryblokContentDeliveryApiHttpClientOptions())
	{
	}

	public StoryblokContentDeliveryApiOptions(
		StoryblokContentDeliveryApiHttpClientOptions client,
		StoryblokContentDeliveryApiResilienceOptions? resilience = null,
		StoryblokContentDeliveryApiCacheOptions? cache = null)
	{
		ArgumentNullException.ThrowIfNull(client);

		Clients =
		[
			new StoryblokContentDeliveryApiHttpClientOptions
			{
				Region = client.Region,
				Token = client.Token,
				Cache = new StoryblokContentDeliveryApiCacheOptions
				{
					UseCache = client.Cache.UseCache,
					CvTtl = client.Cache.CvTtl,
				},
			},
		];
		Resilience = resilience ?? new StoryblokContentDeliveryApiResilienceOptions();
		Cache = cache is null
			? new StoryblokContentDeliveryApiCacheOptions()
			: new StoryblokContentDeliveryApiCacheOptions
			{
				UseCache = cache.UseCache,
				CvTtl = cache.CvTtl,
			};
	}

	public StoryblokContentDeliveryApiOptions(
		string token,
		StoryblokContentDeliveryApiResilienceOptions? resilience = null,
		StoryblokContentDeliveryApiCacheOptions? cache = null)
		: this(
			new StoryblokContentDeliveryApiHttpClientOptions
			{
				Token = token,
			},
			resilience,
			cache)
	{
	}

	public StoryblokContentDeliveryApiOptions(
		IList<StoryblokContentDeliveryApiHttpClientOptions> clients,
		StoryblokContentDeliveryApiResilienceOptions? resilience = null,
		StoryblokContentDeliveryApiCacheOptions? cache = null)
	{
		ArgumentNullException.ThrowIfNull(clients);

		if (clients.Count == 0)
		{
			throw new ArgumentException("At least one client configuration is required.", nameof(clients));
		}

		Clients = clients
			.Select(client => client is null
				? throw new ArgumentException("Client configurations cannot contain null values.", nameof(clients))
				: new StoryblokContentDeliveryApiHttpClientOptions
				{
					Region = client.Region,
					Token = client.Token,
					Cache = new StoryblokContentDeliveryApiCacheOptions
					{
						UseCache = client.Cache.UseCache,
						CvTtl = client.Cache.CvTtl,
					},
				})
			.ToList();

		Resilience = resilience ?? new StoryblokContentDeliveryApiResilienceOptions();
		Cache = cache is null
			? new StoryblokContentDeliveryApiCacheOptions()
			: new StoryblokContentDeliveryApiCacheOptions
			{
				UseCache = cache.UseCache,
				CvTtl = cache.CvTtl,
			};
	}
}
