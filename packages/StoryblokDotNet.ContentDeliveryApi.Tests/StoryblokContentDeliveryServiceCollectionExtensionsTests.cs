using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
}
