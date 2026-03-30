using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryServiceCollectionExtensionsTests
{
	[Fact]
	public void AddStoryblokContentDeliveryApi_WithoutOptions_RegistersApiClientWithDefaultRegion()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

		Assert.Equal(StoryblokRegion.Eu, apiClient.Region);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithConfigureDelegate_UsesConfiguredRegionForRegisteredApiClient()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.China,
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

		Assert.Equal(StoryblokRegion.China, apiClient.Region);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithConfiguration_BindsOptionsFromConfiguration()
	{
		Dictionary<string, string?> settings = new()
		{
			["Storyblok:ContentDelivery:Region"] = nameof(StoryblokRegion.Canada),
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(settings)
			.Build();

		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(configuration.GetSection("Storyblok:ContentDelivery"));

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

		Assert.Equal(StoryblokRegion.Canada, apiClient.Region);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithInvalidRegion_ThrowsOptionsValidationException()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = (StoryblokRegion)999,
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();

		Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>());
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_Called_RegistersKeyedApiClientsForMultipleRegions()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Eu,
			});
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Us,
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient euClient = serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Eu);
		StoryblokContentDeliveryApiClient usClient = serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us);

		Assert.Equal(StoryblokRegion.Eu, euClient.Region);
		Assert.Equal(StoryblokRegion.Us, usClient.Region);
		Assert.NotSame(euClient, usClient);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithDefaultRegion_DefaultAndKeyedClientsCanCoexist()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Canada,
			});
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Australia,
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient defaultClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();
		StoryblokContentDeliveryApiClient australiaClient = serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Australia);

		Assert.Equal(StoryblokRegion.Canada, defaultClient.Region);
		Assert.Equal(StoryblokRegion.Australia, australiaClient.Region);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithSingleConfiguredClient_UnconfiguredKeyedClientThrowsInvalidOperationException()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();

		InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
			() => serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us));

		Assert.Contains("No keyed service for type", exception.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithInvalidResilienceOptions_ThrowsOptionsValidationException()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Resilience.MaxRetryAttempts = -1;
			options.Resilience.BackoffMultiplier = 0.5;
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();

		Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>());
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithCustomResilienceOptions_BindsConfiguration()
	{
		Dictionary<string, string?> settings = new()
		{
			["Storyblok:ContentDelivery:Resilience:MaxRetryAttempts"] = "5",
			["Storyblok:ContentDelivery:Resilience:UseJitter"] = "false",
			["Storyblok:ContentDelivery:Resilience:RespectRetryAfterHeader"] = "false",
		};

		IConfiguration configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(settings)
			.Build();

		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(configuration.GetSection("Storyblok:ContentDelivery"));

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IOptions<StoryblokContentDeliveryApiOptions> options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>();

		Assert.Equal(5, options.Value.Resilience.MaxRetryAttempts);
		Assert.False(options.Value.Resilience.UseJitter);
		Assert.False(options.Value.Resilience.RespectRetryAfterHeader);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithApiOptionsDelegate_ExecutesDelegateOnce()
	{
		int executionCount = 0;
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options =>
		{
			executionCount++;
			options.Cache.UseCache = false;
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Australia,
				Token = "configured-token",
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IOptions<StoryblokContentDeliveryApiOptions> options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>();

		Assert.Equal(1, executionCount);
		Assert.False(options.Value.Cache.UseCache);
		Assert.Single(options.Value.Clients);
		Assert.Equal(StoryblokRegion.Australia, options.Value.Clients[0].Region);
		Assert.Equal("configured-token", options.Value.Clients[0].Token);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithoutHybridCache_RegistersHybridCache()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		HybridCache hybridCache = serviceProvider.GetRequiredService<HybridCache>();

		Assert.NotNull(hybridCache);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithUseCacheDisabled_DoesNotRegisterHybridCache()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi((StoryblokContentDeliveryApiOptions options) => options.Cache.UseCache = false);

		bool hasHybridCacheRegistration = services.Any(serviceDescriptor => serviceDescriptor.ServiceType == typeof(HybridCache));

		Assert.False(hasHybridCacheRegistration);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithUseCacheDisabled_RegistersNoOpApiCache()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi((StoryblokContentDeliveryApiOptions options) => options.Cache.UseCache = false);

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IStoryblokContentDeliveryApiCache cache = serviceProvider.GetRequiredService<IStoryblokContentDeliveryApiCache>();

		Assert.IsType<StoryblokContentDeliveryNoOpApiCache>(cache);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithPreRegisteredHybridCache_DoesNotAddAdditionalHybridCacheRegistrations()
	{
		ServiceCollection services = new();
		services.AddHybridCache();
		int beforeCount = services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(HybridCache));

		services.AddStoryblokContentDeliveryApi();

		int afterCount = services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(HybridCache));

		Assert.Equal(beforeCount, afterCount);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_CalledTwice_DoesNotDuplicateCoreRegistrations()
	{
		ServiceCollection services = new();

		services.AddStoryblokContentDeliveryApi();
		services.AddStoryblokContentDeliveryApi();

		int unkeyedApiClientRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryApiClient)
			&& serviceDescriptor.ServiceKey is null);
		int keyedApiClientRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryApiClient)
			&& serviceDescriptor.ServiceKey is StoryblokRegion);
		int optionsConfigureRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(IConfigureOptions<StoryblokContentDeliveryApiOptions>));
		int optionsPostConfigureRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(IPostConfigureOptions<StoryblokContentDeliveryApiOptions>));
		int apiOptionsValidatorRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(IValidateOptions<StoryblokContentDeliveryApiOptions>));
		int cacheRegistrations = services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IStoryblokContentDeliveryApiCache));

		Assert.Equal(1, unkeyedApiClientRegistrations);
		Assert.Equal(1, keyedApiClientRegistrations);
		Assert.Equal(1, optionsConfigureRegistrations);
		Assert.Equal(1, optionsPostConfigureRegistrations);
		Assert.Equal(1, apiOptionsValidatorRegistrations);
		Assert.Equal(1, cacheRegistrations);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithExternalConfigure_PreservesExternalContributor()
	{
		ServiceCollection services = new();
		services.Configure<StoryblokContentDeliveryApiOptions>(options =>
		{
			options.Resilience.MaxRetryAttempts = 12;
		});

		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Resilience.MaxRetryAttempts = 3;
		});

		int configureContributorCount = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(IConfigureOptions<StoryblokContentDeliveryApiOptions>));

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IOptions<StoryblokContentDeliveryApiOptions> options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>();

		Assert.Equal(2, configureContributorCount);
		Assert.Equal(3, options.Value.Resilience.MaxRetryAttempts);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithExternalPostConfigure_PreservesExternalContributorAcrossRepeatedRegistration()
	{
		ServiceCollection services = new();

		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Resilience.MaxRetryAttempts = 2;
		});

		services.PostConfigure<StoryblokContentDeliveryApiOptions>(options =>
		{
			options.Resilience.MaxRetryAttempts = 11;
		});

		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Resilience.MaxRetryAttempts = 5;
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IOptions<StoryblokContentDeliveryApiOptions> options = serviceProvider.GetRequiredService<IOptions<StoryblokContentDeliveryApiOptions>>();

		Assert.Equal(11, options.Value.Resilience.MaxRetryAttempts);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_CalledTwiceWithDifferentCacheSettings_UsesLastCallCacheConfiguration()
	{
		ServiceCollection services = new();

		services.AddStoryblokContentDeliveryApi();
		services.AddStoryblokContentDeliveryApi(options => options.Cache.UseCache = false);

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IStoryblokContentDeliveryApiCache cache = serviceProvider.GetRequiredService<IStoryblokContentDeliveryApiCache>();

		Assert.IsType<StoryblokContentDeliveryNoOpApiCache>(cache);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_CalledTwiceWithDifferentRegions_RegistersOnlyLatestConfiguredKeyedRegions()
	{
		ServiceCollection services = new();

		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Eu,
			});
		});

		services.AddStoryblokContentDeliveryApi(options =>
		{
			options.Clients.Clear();
			options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
			{
				Region = StoryblokRegion.Us,
			});
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient usClient = serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us);

		Assert.Equal(StoryblokRegion.Us, usClient.Region);
		Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Eu));
	}
}
