using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StoryblokDotNet.ContentDeliveryApi.Tests;

public sealed class StoryblokContentDeliveryServiceCollectionExtensionsTests
{
	[Fact]
	public void AddStoryblokContentDeliveryHttpClientFactory_Called_RegistersResolvableSingletonFactory()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryHttpClientFactory();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryHttpClientFactory firstFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();
		StoryblokContentDeliveryHttpClientFactory secondFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();

		StoryblokContentDeliveryHttpClient firstClient = firstFactory.Create(new StoryblokContentDeliveryHttpClientOptions
		{
			Region = StoryblokRegion.China,
		});
		StoryblokContentDeliveryHttpClient secondClient = secondFactory.Create(new StoryblokContentDeliveryHttpClientOptions
		{
			Region = StoryblokRegion.China,
		});

		Assert.Same(firstFactory, secondFactory);
		Assert.Same(firstClient, secondClient);
		Assert.Equal(new Uri("https://app.storyblokchina.cn/v2/cdn"), firstClient.HttpClient.BaseAddress);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithoutOptions_RegistersApiClientWithDefaultRegion()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi();

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();
		StoryblokContentDeliveryHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<StoryblokContentDeliveryHttpClientFactory>();

		Assert.Equal(StoryblokRegion.Eu, apiClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://api.storyblok.com/v2/cdn"), apiClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
		Assert.Same(apiClient.ContentDeliveryHttpClient, httpClientFactory.Create());
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

		Assert.Equal(StoryblokRegion.Us, apiClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://api-us.storyblok.com/v2/cdn"), apiClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
	}

	[Fact]
	public void AddStoryblokContentDeliveryApi_WithConfigureDelegate_UsesConfiguredRegionForRegisteredApiClient()
	{
		ServiceCollection services = new();
		services.AddStoryblokContentDeliveryApi(options => options.Region = StoryblokRegion.China);

		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		StoryblokContentDeliveryApiClient apiClient = serviceProvider.GetRequiredService<StoryblokContentDeliveryApiClient>();

		Assert.Equal(StoryblokRegion.China, apiClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://app.storyblokchina.cn/v2/cdn"), apiClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
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

		Assert.Equal(StoryblokRegion.Canada, apiClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://api-ca.storyblok.com/v2/cdn"), apiClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
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

		Assert.Equal(StoryblokRegion.Eu, euClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(StoryblokRegion.Us, usClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://api.storyblok.com/v2/cdn"), euClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
		Assert.Equal(new Uri("https://api-us.storyblok.com/v2/cdn"), usClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
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

		Assert.Equal(StoryblokRegion.Canada, defaultClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(StoryblokRegion.Australia, australiaClient.ContentDeliveryHttpClient.Options.Region);
		Assert.Equal(new Uri("https://api-ca.storyblok.com/v2/cdn"), defaultClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
		Assert.Equal(new Uri("https://api-ap.storyblok.com/v2/cdn"), australiaClient.ContentDeliveryHttpClient.HttpClient.BaseAddress);
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

		Assert.Contains("No Storyblok client configuration was supplied for region 'Us'.", exception.Message);
	}
}
