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
		services.Configure<StoryblokContentDeliveryHttpClientOptions>(configuredOptions =>
		{
			configuredOptions.Region = resolvedOptions.Region;
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
		services.Configure<StoryblokContentDeliveryHttpClientOptions>(configuration);

		return services;
	}

	private static IServiceCollection AddStoryblokContentDeliveryApiCore(IServiceCollection services)
	{
		services
			.AddOptions<StoryblokContentDeliveryHttpClientOptions>()
			.Validate(
				static options => Enum.IsDefined(options.Region),
				$"{nameof(StoryblokContentDeliveryHttpClientOptions.Region)} must be a valid {nameof(StoryblokRegion)} value.")
			.ValidateOnStart();
		services.AddStoryblokContentDeliveryHttpClientFactory();
		services.AddSingleton(serviceProvider =>
		{
			StoryblokContentDeliveryHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();
			StoryblokContentDeliveryHttpClientOptions options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryHttpClientOptions>>().Value;
			return new StoryblokContentDeliveryApiClient(httpClientFactory, options);
		});

		foreach (StoryblokRegion region in StoryblokContentDeliveryHttpClientFactory.Regions)
		{
			services.AddKeyedSingleton<StoryblokContentDeliveryApiClient>(
				region,
				static (serviceProvider, serviceKey) =>
				{
					StoryblokContentDeliveryHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();
					StoryblokRegion resolvedRegion = serviceKey is StoryblokRegion regionKey
						? regionKey
						: throw new InvalidOperationException("Storyblok API client service key must be a StoryblokRegion value.");

					return new StoryblokContentDeliveryApiClient(
						httpClientFactory,
						new StoryblokContentDeliveryHttpClientOptions
						{
							Region = resolvedRegion,
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
