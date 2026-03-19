using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;

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
	public void AddStoryblokContentDeliveryResilience_WithManualHttpClientBuilder_RegistersNamedClient()
	{
		ServiceCollection services = new();
		services
			.AddHttpClient("manual-storyblok")
			.AddStoryblokContentDeliveryResilience(new StoryblokContentDeliveryResilienceOptions
			{
				MaxRetryAttempts = 1,
				UseJitter = false,
			});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

		HttpClient client = httpClientFactory.CreateClient("manual-storyblok");

		Assert.NotNull(client);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithOptions_UsesConfiguredRegionForRegisteredApiClient()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(new StoryblokContentDeliveryHttpClientOptions
		{
			Region = StoryblokRegion.Us,
		});

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

		Assert.Equal(StoryblokRegion.Us, apiClient.Region);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithConfigureDelegate_UsesConfiguredRegionForRegisteredApiClient()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options => options.Region = StoryblokRegion.China);

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
		services.AddStoryblokContentDeliveryApi(options => options.Region = (StoryblokRegion)999);

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
	public void AddStoryblokContentDeliveryApi_WithOptionsPattern_DuplicateRegionsThrowOptionsValidationException()
	{
		ServiceCollection services = new();
		services
			.AddOptions<StoryblokContentDeliveryApiOptions>()
			.Configure(options =>
			{
				options.Clients.Clear();
				options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
				{
					Region = StoryblokRegion.China,
				});
				options.Clients.Add(new StoryblokContentDeliveryHttpClientOptions
				{
					Region = StoryblokRegion.China,
				});
			});
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();

		Assert.Throws<OptionsValidationException>(() => serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>());
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithSingleConfiguredClient_UnconfiguredKeyedClientThrowsInvalidOperationException()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();

		InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
			() => serviceProvider.GetRequiredKeyedService<StoryblokContentDeliveryApiClient>(StoryblokRegion.Us));

		Assert.Contains("No Storyblok client configuration was supplied for region 'Us'.", exception.Message, StringComparison.Ordinal);
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
	public void AddStoryblokContentDeliveryApi_CalledTwice_DoesNotDuplicateCoreRegistrations()
	{
		ServiceCollection services = new();

		services.AddStoryblokContentDeliveryApi();
		services.AddStoryblokContentDeliveryApi();

		int factoryRegistrations = services.Count(serviceDescriptor => serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryHttpClientFactory));
		int unkeyedApiClientRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryApiClient)
			&& serviceDescriptor.ServiceKey is null);
		int keyedApiClientRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(StoryblokContentDeliveryApiClient)
			&& serviceDescriptor.ServiceKey is StoryblokRegion);
		int apiOptionsValidatorRegistrations = services.Count(serviceDescriptor =>
			serviceDescriptor.ServiceType == typeof(IValidateOptions<StoryblokContentDeliveryApiOptions>));

		Assert.Equal(1, factoryRegistrations);
		Assert.Equal(1, unkeyedApiClientRegistrations);
		Assert.Equal(StoryblokContentDeliveryHttpClientFactory.Regions.Count, keyedApiClientRegistrations);
		Assert.Equal(1, apiOptionsValidatorRegistrations);
	}

	[Fact]
	public void ResolveRetryDelay_WithFirstRetryWithoutRetryAfter_UsesInitialDelay()
	{
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			InitialDelay = TimeSpan.FromMilliseconds(250),
			UseJitter = false,
		};

		TimeSpan retryDelay = StoryblokContentDeliveryServiceCollectionExtensions.ResolveRetryDelay(0, null, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(250), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectEnabled_UsesRetryAfterValue()
	{
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = true,
			MaxDelay = TimeSpan.FromSeconds(10),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryServiceCollectionExtensions.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromSeconds(3), retryDelay);
	}

	[Fact]
	public void ResolveRetryDelay_WithRetryAfterAndRespectDisabled_UsesBackoffDelay()
	{
		StoryblokContentDeliveryResilienceOptions resilienceOptions = new()
		{
			UseJitter = false,
			RespectRetryAfterHeader = false,
			InitialDelay = TimeSpan.FromMilliseconds(400),
		};
		using HttpResponseMessage response = new(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(3));

		TimeSpan retryDelay = StoryblokContentDeliveryServiceCollectionExtensions.ResolveRetryDelay(0, response, resilienceOptions);

		Assert.Equal(TimeSpan.FromMilliseconds(400), retryDelay);
	}
}
