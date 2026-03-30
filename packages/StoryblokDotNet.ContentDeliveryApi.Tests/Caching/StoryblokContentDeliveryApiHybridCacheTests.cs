using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Caching;

public sealed class StoryblokContentDeliveryApiHybridCacheTests
{
	[Fact]
	public async Task GetOrCreate_WithTagAndClearByTag_RefreshesCachedValue()
	{
		HybridCache hybridCache = CreateHybridCache(out IOptions<HybridCacheOptions> hybridCacheOptions);
		StoryblokContentDeliveryApiHybridCache sut = new(hybridCache, hybridCacheOptions);
		StoryblokContentDeliveryRequest request = CreateRequest();
		StoryblokContentDeliveryCacheEntryOptions cacheOptions = new();
		cacheOptions.Tags.Add("stories");
		int invocationCount = 0;

		Task<StoryblokContentDeliveryResult<string>> ValueFactory(CancellationToken cancellationToken)
		{
			invocationCount++;
			return Task.FromResult(StoryblokContentDeliveryResult<string>.Success($"value-{invocationCount}"));
		}

		StoryblokContentDeliveryResult<string> first = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cacheOptions, TestContext.Current.CancellationToken);
		StoryblokContentDeliveryResult<string> second = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cacheOptions, TestContext.Current.CancellationToken);
		await sut.ClearByTag("stories", TestContext.Current.CancellationToken);
		StoryblokContentDeliveryResult<string> third = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cacheOptions, TestContext.Current.CancellationToken);

		Assert.True(first.IsSuccess);
		Assert.True(second.IsSuccess);
		Assert.True(third.IsSuccess);
		Assert.Equal("value-1", first.Data);
		Assert.Equal("value-1", second.Data);
		Assert.Equal("value-2", third.Data);
		Assert.Equal(2, invocationCount);
	}

	[Fact]
	public async Task GetOrCreate_WithTagAndClearAll_RefreshesCachedValue()
	{
		HybridCache hybridCache = CreateHybridCache(out IOptions<HybridCacheOptions> hybridCacheOptions);
		StoryblokContentDeliveryApiHybridCache sut = new(hybridCache, hybridCacheOptions);
		StoryblokContentDeliveryRequest request = CreateRequest();
		StoryblokContentDeliveryCacheEntryOptions cacheOptions = new();
		cacheOptions.Tags.Add("stories");
		int invocationCount = 0;

		Task<StoryblokContentDeliveryResult<string>> ValueFactory(CancellationToken cancellationToken)
		{
			invocationCount++;
			return Task.FromResult(StoryblokContentDeliveryResult<string>.Success($"value-{invocationCount}"));
		}

		StoryblokContentDeliveryResult<string> first = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cacheOptions, TestContext.Current.CancellationToken);
		await sut.ClearAll(TestContext.Current.CancellationToken);
		StoryblokContentDeliveryResult<string> second = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cacheOptions, TestContext.Current.CancellationToken);

		Assert.True(first.IsSuccess);
		Assert.True(second.IsSuccess);
		Assert.Equal("value-1", first.Data);
		Assert.Equal("value-2", second.Data);
		Assert.Equal(2, invocationCount);
	}

	private static HybridCache CreateHybridCache(out IOptions<HybridCacheOptions> options)
	{
		ServiceCollection services = new();
		services.AddHybridCache();

		ServiceProvider serviceProvider = services.BuildServiceProvider();
		options = serviceProvider.GetRequiredService<IOptions<HybridCacheOptions>>();
		return serviceProvider.GetRequiredService<HybridCache>();
	}

	[Fact]
	public async Task GetOrCreate_WithoutHybridCacheOptions_UsesDefaultMaximumKeyLength()
	{
		HybridCache hybridCache = CreateHybridCache(out _);
		StoryblokContentDeliveryApiHybridCache sut = new(hybridCache);
		StoryblokContentDeliveryRequest request = CreateRequest();
		int invocationCount = 0;

		Task<StoryblokContentDeliveryResult<string>> ValueFactory(CancellationToken cancellationToken)
		{
			invocationCount++;
			return Task.FromResult(StoryblokContentDeliveryResult<string>.Success($"value-{invocationCount}"));
		}

		StoryblokContentDeliveryResult<string> first = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cancellationToken: TestContext.Current.CancellationToken);
		StoryblokContentDeliveryResult<string> second = await sut.GetOrCreate(StoryblokRegion.Eu, request, ValueFactory, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(first.IsSuccess);
		Assert.True(second.IsSuccess);
		Assert.Equal("value-1", first.Data);
		Assert.Equal("value-1", second.Data);
		Assert.Equal(1, invocationCount);
	}

	private static StoryblokContentDeliveryRequest CreateRequest()
	{
		StoryblokContentDeliveryQuery query = new()
		{
			Token = "token-value",
		};

		return new StoryblokContentDeliveryRequest("stories", query);
	}
}
