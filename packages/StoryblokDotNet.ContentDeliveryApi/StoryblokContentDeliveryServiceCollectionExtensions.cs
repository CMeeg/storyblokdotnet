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

		return AddStoryblokContentDeliveryApi(services, static _ =>
		{
		});
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		Action<StoryblokContentDeliveryApiOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configureOptions);

		StoryblokContentDeliveryApiOptions resolvedOptions = new();
		configureOptions(resolvedOptions);

		return AddStoryblokContentDeliveryApiCore(services, resolvedOptions);
	}

	public static IServiceCollection AddStoryblokContentDeliveryApi(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		StoryblokContentDeliveryApiOptions resolvedOptions = new();

		if (configuration.GetSection("Clients").Exists())
		{
			resolvedOptions.Clients.Clear();
			configuration.Bind(resolvedOptions);
		}
		else
		{
			resolvedOptions.Clients.Clear();

			StoryblokContentDeliveryHttpClientOptions defaultClientOptions = new();
			configuration.Bind(defaultClientOptions);
			resolvedOptions.Clients.Add(defaultClientOptions);

			IConfigurationSection resilienceSection = configuration.GetSection(nameof(StoryblokContentDeliveryApiOptions.Resilience));
			if (resilienceSection.Exists())
			{
				resilienceSection.Bind(resolvedOptions.Resilience);
			}

			IConfigurationSection cacheSection = configuration.GetSection(nameof(StoryblokContentDeliveryApiOptions.Cache));
			if (cacheSection.Exists())
			{
				cacheSection.Bind(resolvedOptions.Cache);
			}
		}

		return AddStoryblokContentDeliveryApiCore(services, resolvedOptions);
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
		StoryblokContentDeliveryApiOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		services
			.AddOptions<StoryblokContentDeliveryApiOptions>()
			.ValidateOnStart();

		services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<StoryblokContentDeliveryApiOptions>, StoryblokContentDeliveryApiOptionsValidator>());

		RemoveStoryblokOptionsContributors(services);
		services.AddSingleton<IConfigureOptions<StoryblokContentDeliveryApiOptions>>(new StoryblokContentDeliveryApiOptionsSetup(options));
		services.AddSingleton<IPostConfigureOptions<StoryblokContentDeliveryApiOptions>>(new StoryblokContentDeliveryApiOptionsPostSetup());

		services.RemoveAll<IStoryblokContentDeliveryApiCache>();

		if (options.Cache.UseCache && services.All(static serviceDescriptor => serviceDescriptor.ServiceType != typeof(HybridCache)))
		{
			services.AddHybridCache();
		}

		if (options.Cache.UseCache)
		{
			services.AddSingleton<IStoryblokContentDeliveryApiCache>(serviceProvider =>
			{
				HybridCache hybridCache = serviceProvider.GetRequiredService<HybridCache>();
				IOptions<HybridCacheOptions>? hybridCacheOptions = serviceProvider.GetService<IOptions<HybridCacheOptions>>();
				return new StoryblokContentDeliveryApiHybridCache(hybridCache, hybridCacheOptions);
			});
		}
		else
		{
			services.AddSingleton<IStoryblokContentDeliveryApiCache>(StoryblokContentDeliveryNoOpApiCache.Instance);
		}

		if (services.All(static serviceDescriptor => serviceDescriptor.ServiceType != typeof(StoryblokContentDeliveryApiHttpClientRegistrationMarker)))
		{
			services.AddHttpClient(StoryblokContentDeliveryApiClient.HttpClientName)
				.AddStoryblokContentDeliveryResilience();

			services.AddSingleton<StoryblokContentDeliveryApiHttpClientRegistrationMarker>();
		}

		for (int index = services.Count - 1; index >= 0; index--)
		{
			if (services[index].ServiceType == typeof(StoryblokContentDeliveryApiClient))
			{
				services.RemoveAt(index);
			}
		}

		services.AddSingleton(serviceProvider =>
		{
			IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
			StoryblokContentDeliveryApiOptions resolvedOptions = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>().Value;
			IStoryblokContentDeliveryApiCache cache = serviceProvider.GetRequiredService<IStoryblokContentDeliveryApiCache>();
			return new StoryblokContentDeliveryApiClient(resolvedOptions, httpClientFactory, cache);
		});

		foreach (StoryblokRegion region in options.Clients.Select(static client => client.Region).Distinct())
		{
			StoryblokRegion resolvedRegion = region;
			services.AddKeyedSingleton<StoryblokContentDeliveryApiClient>(
				resolvedRegion,
				(serviceProvider, _) =>
				{
					StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();
					return apiClient.ForRegion(resolvedRegion);
				});
		}

		return services;
	}

	private static void RemoveStoryblokOptionsContributors(IServiceCollection services)
	{
		for (int index = services.Count - 1; index >= 0; index--)
		{
			ServiceDescriptor serviceDescriptor = services[index];
			if (serviceDescriptor.ServiceType == typeof(IConfigureOptions<StoryblokContentDeliveryApiOptions>)
				&& serviceDescriptor.ImplementationInstance is StoryblokContentDeliveryApiOptionsSetup)
			{
				services.RemoveAt(index);
				continue;
			}

			if (serviceDescriptor.ServiceType == typeof(IPostConfigureOptions<StoryblokContentDeliveryApiOptions>)
				&& serviceDescriptor.ImplementationInstance is StoryblokContentDeliveryApiOptionsPostSetup)
			{
				services.RemoveAt(index);
			}
		}
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

	private sealed class StoryblokContentDeliveryApiHttpClientRegistrationMarker
	{
	}

	private sealed class StoryblokContentDeliveryApiOptionsSetup : IConfigureOptions<StoryblokContentDeliveryApiOptions>
	{
		private readonly StoryblokContentDeliveryApiOptions sourceOptions;

		public StoryblokContentDeliveryApiOptionsSetup(StoryblokContentDeliveryApiOptions sourceOptions)
		{
			this.sourceOptions = sourceOptions;
		}

		public void Configure(StoryblokContentDeliveryApiOptions options)
		{
			CopyOptions(sourceOptions, options);
		}
	}

	private sealed class StoryblokContentDeliveryApiOptionsPostSetup : IPostConfigureOptions<StoryblokContentDeliveryApiOptions>
	{
		public void PostConfigure(string? name, StoryblokContentDeliveryApiOptions options)
		{
		}
	}
}
