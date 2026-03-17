using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StoryblokDotNet.ContentDeliveryApi;

public static class StoryblokContentDeliveryServiceCollectionExtensions
{
	public static IServiceCollection AddStoryblokContentDeliveryApi(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		return AddStoryblokContentDeliveryApiCore(services);
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		AddStoryblokContentDeliveryApiCore(services);
		services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
		{
			configuredOptions.Clients.Clear();
			configuredOptions.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = resolvedOptions.Region,
			});
		});

		return services;
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		Action<StoryblokContentDeliveryHttpClientOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configureOptions);

		AddStoryblokContentDeliveryApiCore(services);
		services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
		{
			configuredOptions.Clients.Clear();
			StoryblokContentDeliveryHttpClientOptions defaultClientOptions = new();
			configureOptions(defaultClientOptions);
			configuredOptions.Clients.Add(defaultClientOptions);
		});

		return services;
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		Action<StoryblokContentDeliveryApiOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configureOptions);

		AddStoryblokContentDeliveryApiCore(services);
		services.Configure(configureOptions);

		return services;
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		AddStoryblokContentDeliveryApiCore(services);

		if (configuration.GetSection("Clients").Exists())
		{
			services.Configure<StoryblokContentDeliveryApiOptions>(configuration);
		}
		else
		{
			services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
			{
				configuredOptions.Clients.Clear();

				StoryblokContentDeliveryHttpClientOptions defaultClientOptions = new();
				configuration.Bind(defaultClientOptions);
				configuredOptions.Clients.Add(defaultClientOptions);
			});
		}

		return services;
	}

	private static IServiceCollection AddStoryblokContentDeliveryApiCore(IServiceCollection services)
	{
		services
			.AddOptions<StoryblokContentDeliveryApiOptions>()
			.Validate(
				static options => options.Clients.Count > 0,
				$"{nameof(StoryblokContentDeliveryApiOptions.Clients)} must include at least one client configuration.")
			.Validate(
				static options => options.Clients.All(client => Enum.IsDefined(client.Region)),
				$"{nameof(StoryblokContentDeliveryHttpClientOptions.Region)} must be a valid {nameof(StoryblokRegion)} value.")
			.Validate(
				static options =>
				{
					HashSet<StoryblokRegion> configuredRegions = [];
					return options.Clients.All(client => configuredRegions.Add(client.Region));
				},
				$"{nameof(StoryblokContentDeliveryApiOptions.Clients)} can include at most one configuration per {nameof(StoryblokRegion)} value.")
			.ValidateOnStart();
		services.AddStoryblokContentDeliveryHttpClientFactory();
		services.AddSingleton(serviceProvider =>
		{
			StoryblokContentDeliveryHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();
			StoryblokContentDeliveryApiOptions options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>().Value;
			StoryblokContentDeliveryHttpClientOptions defaultOptions = options.Clients[0];
			return new StoryblokContentDeliveryApiClient(httpClientFactory, defaultOptions);
		});

		foreach (StoryblokRegion region in StoryblokContentDeliveryHttpClientFactory.Regions)
		{
			services.AddKeyedSingleton<StoryblokContentDeliveryApiClient>(
				region,
				static (serviceProvider, serviceKey) =>
				{
					StoryblokContentDeliveryHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();
					StoryblokContentDeliveryApiOptions options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>().Value;
					StoryblokRegion resolvedRegion = serviceKey is StoryblokRegion regionKey
						? regionKey
						: throw new InvalidOperationException("Storyblok API client service key must be a StoryblokRegion value.");
					StoryblokContentDeliveryHttpClientOptions configuredClientOptions = options.Clients.FirstOrDefault(client => client.Region == resolvedRegion)
						?? throw new InvalidOperationException($"No Storyblok client configuration was supplied for region '{resolvedRegion}'.");

					return new StoryblokContentDeliveryApiClient(
						httpClientFactory,
						new StoryblokContentDeliveryHttpClientOptions
						{
							Region = configuredClientOptions.Region,
						});
				});
		}

		return services;
	}

	public static IServiceCollection AddStoryblokContentDeliveryHttpClientFactory(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		foreach (StoryblokRegion region in StoryblokContentDeliveryHttpClientFactory.Regions)
		{
			services.AddHttpClient(
				StoryblokContentDeliveryHttpClientFactory.GetClientName(region),
				client => client.BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(region));
		}

		services.AddSingleton<StoryblokContentDeliveryHttpClientFactory>();

		return services;
	}
}
