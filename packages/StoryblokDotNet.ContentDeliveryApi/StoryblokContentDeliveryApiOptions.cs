using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryApiOptions
{
	public IList<StoryblokContentDeliveryHttpClientOptions> Clients { get; }

	public StoryblokContentDeliveryResilienceOptions Resilience { get; }

	public bool UseCache { get; set; } = true;

	public StoryblokContentDeliveryApiOptions()
		: this(new StoryblokContentDeliveryHttpClientOptions())
	{
	}

	public StoryblokContentDeliveryApiOptions(
		StoryblokContentDeliveryHttpClientOptions client,
		StoryblokContentDeliveryResilienceOptions? resilience = null)
	{
		ArgumentNullException.ThrowIfNull(client);

		Clients =
		[
			new StoryblokContentDeliveryHttpClientOptions
			{
				Region = client.Region,
				Token = client.Token,
			},
		];
		Resilience = resilience ?? new StoryblokContentDeliveryResilienceOptions();
		UseCache = true;
	}

	public StoryblokContentDeliveryApiOptions(
		string token,
		StoryblokContentDeliveryResilienceOptions? resilience = null)
		: this(
			new StoryblokContentDeliveryHttpClientOptions
			{
				Token = token,
			},
			resilience)
	{
	}

	public StoryblokContentDeliveryApiOptions(
		IList<StoryblokContentDeliveryHttpClientOptions> clients,
		StoryblokContentDeliveryResilienceOptions? resilience = null)
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
				})
			.ToList();

		Resilience = resilience ?? new StoryblokContentDeliveryResilienceOptions();
		UseCache = true;
	}
}
