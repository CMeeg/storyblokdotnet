using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiOptions
{
	public IList<StoryblokContentDeliveryHttpClientOptions> Clients { get; }

	public StoryblokContentDeliveryResilienceOptions Resilience { get; }

	public StoryblokContentDeliveryCacheOptions Cache { get; }

	public StoryblokContentDeliveryApiOptions()
		: this(new StoryblokContentDeliveryHttpClientOptions())
	{
	}

	public StoryblokContentDeliveryApiOptions(
		StoryblokContentDeliveryHttpClientOptions client,
		StoryblokContentDeliveryResilienceOptions? resilience = null,
		StoryblokContentDeliveryCacheOptions? cache = null)
	{
		ArgumentNullException.ThrowIfNull(client);

		Clients =
		[
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = client.Region,
				Token = client.Token,
				Cache = new StoryblokContentDeliveryCacheOptions
				{
					UseCache = client.Cache.UseCache,
					CvTtl = client.Cache.CvTtl,
				},
			},
		];
		Resilience = resilience ?? new StoryblokContentDeliveryResilienceOptions();
		Cache = cache is null
			? new StoryblokContentDeliveryCacheOptions()
			: new StoryblokContentDeliveryCacheOptions
			{
				UseCache = cache.UseCache,
				CvTtl = cache.CvTtl,
			};
	}

	public StoryblokContentDeliveryApiOptions(
		string token,
		StoryblokContentDeliveryResilienceOptions? resilience = null,
		StoryblokContentDeliveryCacheOptions? cache = null)
		: this(
			new StoryblokContentDeliveryHttpClientOptions
			{
				Token = token,
			},
			resilience,
			cache)
	{
	}

	public StoryblokContentDeliveryApiOptions(
		IList<StoryblokContentDeliveryHttpClientOptions> clients,
		StoryblokContentDeliveryResilienceOptions? resilience = null,
		StoryblokContentDeliveryCacheOptions? cache = null)
	{
		ArgumentNullException.ThrowIfNull(clients);

		if (clients.Count == 0)
		{
			throw new ArgumentException("At least one client configuration is required.", nameof(clients));
		}

		Clients = clients
			.Select(client => client is null
				? throw new ArgumentException("Client configurations cannot contain null values.", nameof(clients))
				: new StoryblokContentDeliveryHttpClientOptions
				{
					Region = client.Region,
					Token = client.Token,
					Cache = new StoryblokContentDeliveryCacheOptions
					{
						UseCache = client.Cache.UseCache,
						CvTtl = client.Cache.CvTtl,
					},
				})
			.ToList();

		Resilience = resilience ?? new StoryblokContentDeliveryResilienceOptions();
		Cache = cache is null
			? new StoryblokContentDeliveryCacheOptions()
			: new StoryblokContentDeliveryCacheOptions
			{
				UseCache = cache.UseCache,
				CvTtl = cache.CvTtl,
			};
	}
}
