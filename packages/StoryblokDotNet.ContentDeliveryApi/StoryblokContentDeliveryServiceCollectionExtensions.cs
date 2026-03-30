using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi;

public static class StoryblokContentDeliveryServiceCollectionExtensions
{
	public static IServiceCollection AddStoryblokContentDeliveryApi(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		return AddStoryblokContentDeliveryApiCore(services, new StoryblokContentDeliveryCacheOptions());
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		AddStoryblokContentDeliveryApiCore(services, new StoryblokContentDeliveryCacheOptions());

		services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
		{
			configuredOptions.Clients.Clear();
			configuredOptions.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = resolvedOptions.Region,
				Token = resolvedOptions.Token,
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

		AddStoryblokContentDeliveryApiCore(services, new StoryblokContentDeliveryCacheOptions());
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

		StoryblokContentDeliveryApiOptions resolvedOptions = new();
		configureOptions(resolvedOptions);

		AddStoryblokContentDeliveryApiCore(services, resolvedOptions.Cache);

		services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions => CopyOptions(resolvedOptions, configuredOptions));

		return services;
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		StoryblokContentDeliveryCacheOptions cacheOptions = new();
		IConfigurationSection cacheSection = configuration.GetSection(nameof(StoryblokContentDeliveryApiOptions.Cache));
		if (cacheSection.Exists())
		{
			cacheSection.Bind(cacheOptions);
		}

		AddStoryblokContentDeliveryApiCore(services, cacheOptions);

		if (configuration.GetSection("Clients").Exists())
		{
			services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
			{
				configuredOptions.Clients.Clear();
				configuration.Bind(configuredOptions);
			});
		}
		else
		{
			services.Configure<StoryblokContentDeliveryApiOptions>(configuredOptions =>
			{
				configuredOptions.Clients.Clear();

				StoryblokContentDeliveryHttpClientOptions defaultClientOptions = new();
				configuration.Bind(defaultClientOptions);
				configuredOptions.Clients.Add(defaultClientOptions);

				IConfigurationSection resilienceSection = configuration.GetSection(nameof(StoryblokContentDeliveryApiOptions.Resilience));
				if (resilienceSection.Exists())
				{
					resilienceSection.Bind(configuredOptions.Resilience);
				}
			});
		}

		return services;
	}

	private static void CopyOptions(
		StoryblokContentDeliveryApiOptions source,
		StoryblokContentDeliveryApiOptions destination)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);

		destination.Clients.Clear();
		foreach (StoryblokContentDeliveryHttpClientOptions client in source.Clients)
		{
			destination.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = client.Region,
				Token = client.Token,
				Cache = new StoryblokContentDeliveryCacheOptions
				{
					UseCache = client.Cache.UseCache,
					CvTtl = client.Cache.CvTtl,
				},
			});
		}

		destination.Resilience.MaxRetryAttempts = source.Resilience.MaxRetryAttempts;
		destination.Resilience.InitialDelay = source.Resilience.InitialDelay;
		destination.Resilience.MaxDelay = source.Resilience.MaxDelay;
		destination.Resilience.BackoffMultiplier = source.Resilience.BackoffMultiplier;
		destination.Resilience.UseJitter = source.Resilience.UseJitter;
		destination.Resilience.RespectRetryAfterHeader = source.Resilience.RespectRetryAfterHeader;
		destination.Cache.UseCache = source.Cache.UseCache;
		destination.Cache.CvTtl = source.Cache.CvTtl;
	}

	private static IServiceCollection AddStoryblokContentDeliveryApiCore(
		IServiceCollection services,
		StoryblokContentDeliveryCacheOptions cacheOptions)
	{
		ArgumentNullException.ThrowIfNull(cacheOptions);

		if (services.Any(static serviceDescriptor => serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryApiRegistrationMarker)))
		{
			return services;
		}

		services
			.AddOptions<StoryblokContentDeliveryApiOptions>()
			.ValidateOnStart();

		services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<StoryblokContentDeliveryApiOptions>, StoryblokContentDeliveryApiOptionsValidator>());

		if (cacheOptions.UseCache && services.All(static serviceDescriptor => serviceDescriptor.ServiceType != typeof(HybridCache)))
		{
			services.AddHybridCache();
		}

		if (cacheOptions.UseCache)
		{
			services.TryAddSingleton<IStoryblokContentDeliveryApiCache>(serviceProvider =>
			{
				HybridCache hybridCache = serviceProvider.GetRequiredService<HybridCache>();
				IOptions<HybridCacheOptions>? options = serviceProvider.GetService<IOptions<HybridCacheOptions>>();
				return new StoryblokContentDeliveryApiHybridCache(hybridCache, options);
			});
		}
		else
		{
			services.TryAddSingleton<IStoryblokContentDeliveryApiCache>(StoryblokContentDeliveryNoOpApiCache.Instance);
		}

		services.AddHttpClient(StoryblokContentDeliveryApiClient.HttpClientName)
			.AddStoryblokContentDeliveryResilience();

		services.TryAddSingleton(serviceProvider =>
		{
			IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			StoryblokContentDeliveryApiOptions options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>().Value;
			IStoryblokContentDeliveryApiCache cache = serviceProvider.GetRequiredService<IStoryblokContentDeliveryApiCache>();
			return new StoryblokContentDeliveryApiClient(options, httpClientFactory, cache);
		});

		foreach (StoryblokRegion region in StoryblokContentDeliveryApiClient.Regions)
		{
			services.AddKeyedSingleton<StoryblokContentDeliveryApiClient>(
				region,
				static (serviceProvider, serviceKey) =>
				{
					StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();
					StoryblokContentDeliveryApiOptions options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>().Value;
					StoryblokRegion resolvedRegion = serviceKey is StoryblokRegion regionKey
						? regionKey
						: throw new InvalidOperationException("Storyblok API client service key must be a StoryblokRegion value.");
					StoryblokContentDeliveryHttpClientOptions? configuredClientOptions = options.Clients.FirstOrDefault(client => client.Region == resolvedRegion);
					bool hasConfigurationForRegion = configuredClientOptions is not null;

					if (!hasConfigurationForRegion)
					{
						throw new InvalidOperationException($"No Storyblok client configuration was supplied for region '{resolvedRegion}'.");
					}

					return apiClient.ForRegion(resolvedRegion);
				});
		}

		services.AddSingleton<StoryblokContentDeliveryApiRegistrationMarker>();

		return services;
	}

	private sealed class StoryblokContentDeliveryApiOptionsValidator : IValidateOptions<StoryblokContentDeliveryApiOptions>
	{
		public ValidateOptionsResult Validate(string? name, StoryblokContentDeliveryApiOptions options)
		{
			List<string> failures = [];

			if (options.Clients.Count == 0)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryApiOptions.Clients)} must include at least one client configuration.");
			}

			if (options.Clients.Any(static client => !Enum.IsDefined(client.Region)))
			{
				failures.Add($"{nameof(StoryblokContentDeliveryHttpClientOptions.Region)} must be a valid {nameof(StoryblokRegion)} value.");
			}

			HashSet<StoryblokRegion> configuredRegions = [];
			if (options.Clients.Any(client => !configuredRegions.Add(client.Region)))
			{
				failures.Add($"{nameof(StoryblokContentDeliveryApiOptions.Clients)} can include at most one configuration per {nameof(StoryblokRegion)} value.");
			}

			if (options.Resilience.MaxRetryAttempts < 0)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryResilienceOptions.MaxRetryAttempts)} must be zero or greater.");
			}

			if (options.Resilience.InitialDelay < TimeSpan.Zero)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryResilienceOptions.InitialDelay)} must be zero or greater.");
			}

			if (options.Resilience.MaxDelay <= TimeSpan.Zero)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryResilienceOptions.MaxDelay)} must be greater than zero.");
			}

			if (options.Resilience.MaxDelay < options.Resilience.InitialDelay)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryResilienceOptions.MaxDelay)} must be greater than or equal to {nameof(StoryblokContentDeliveryResilienceOptions.InitialDelay)}.");
			}

			if (options.Resilience.BackoffMultiplier < 1)
			{
				failures.Add($"{nameof(StoryblokContentDeliveryResilienceOptions.BackoffMultiplier)} must be greater than or equal to 1.");
			}

			return failures.Count == 0
				? ValidateOptionsResult.Success
				: ValidateOptionsResult.Fail(failures);
		}
	}

	private sealed class StoryblokContentDeliveryApiRegistrationMarker
	{
	}
}
