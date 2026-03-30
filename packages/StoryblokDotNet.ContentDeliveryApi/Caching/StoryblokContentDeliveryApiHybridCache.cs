using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace StoryblokDotNet.ContentDeliveryApi.Caching;

internal sealed class StoryblokContentDeliveryApiHybridCache : IStoryblokContentDeliveryApiCache
{
	private const int DefaultMaximumKeyLength = 1024;

	private readonly HybridCache cache;
	private readonly int maximumKeyLength;

	public StoryblokContentDeliveryApiHybridCache(HybridCache cache, IOptions<HybridCacheOptions>? options = null)
	{
		ArgumentNullException.ThrowIfNull(cache);

		this.cache = cache;
		this.maximumKeyLength = options?.Value.MaximumKeyLength ?? DefaultMaximumKeyLength;
	}

	public async Task<StoryblokContentDeliveryResult<TResponse>> GetOrCreate<TResponse>(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		Func<CancellationToken, Task<StoryblokContentDeliveryResult<TResponse>>> valueFactory,
		StoryblokContentDeliveryCacheEntryOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentNullException.ThrowIfNull(valueFactory);

		string cacheKey = StoryblokContentDeliveryApiCacheKeyBuilder.Create(region, request, maximumKeyLength);
		List<string>? tags = GetTags(options);
		HybridCacheEntryOptions? entryOptions = CreateEntryOptions(options);

		try
		{
			TResponse response = await cache.GetOrCreateAsync(
				cacheKey,
				valueFactory,
				static async (factory, cancel) =>
				{
					StoryblokContentDeliveryResult<TResponse> result = await factory(cancel).ConfigureAwait(false);
					if (!result.IsSuccess)
					{
						throw new StoryblokContentDeliveryApiCacheBypassException<TResponse>(result);
					}

					return result.Data;
				},
				entryOptions,
				tags,
				cancellationToken: cancellationToken).ConfigureAwait(false);

			return StoryblokContentDeliveryResult<TResponse>.Success(response);
		}
		catch (StoryblokContentDeliveryApiCacheBypassException<TResponse> exception)
		{
			return exception.Result;
		}
	}

	public async Task Clear(
		StoryblokRegion region,
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		await cache.RemoveAsync(StoryblokContentDeliveryApiCacheKeyBuilder.Create(region, request, maximumKeyLength), cancellationToken).ConfigureAwait(false);
	}

	public async Task ClearByTag(string tag, CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(tag);
		if (string.Equals(tag, "*", StringComparison.Ordinal))
		{
			throw new ArgumentException("The '*' tag is reserved for ClearAll.", nameof(tag));
		}

		await cache.RemoveByTagAsync(tag, cancellationToken).ConfigureAwait(false);
	}

	public async Task ClearAll(CancellationToken cancellationToken = default)
	{
		await cache.RemoveByTagAsync("*", cancellationToken).ConfigureAwait(false);
	}

	private static List<string>? GetTags(StoryblokContentDeliveryCacheEntryOptions? options)
	{
		if (options is null || options.Tags.Count == 0)
		{
			return null;
		}

		List<string> tags = [];
		foreach (string tag in options.Tags)
		{
			if (string.IsNullOrWhiteSpace(tag))
			{
				throw new ArgumentException("Cache tags cannot be null, empty, or whitespace.", nameof(options));
			}

			if (string.Equals(tag, "*", StringComparison.Ordinal))
			{
				throw new ArgumentException("The '*' tag is reserved for ClearAll.", nameof(options));
			}

			tags.Add(tag);
		}

		return tags;
	}

	private static HybridCacheEntryOptions? CreateEntryOptions(StoryblokContentDeliveryCacheEntryOptions? options)
	{
		if (options is null)
		{
			return null;
		}

		if (options.Expiration is null && options.LocalCacheExpiration is null && options.Flags is null)
		{
			return null;
		}

		return new HybridCacheEntryOptions
		{
			Expiration = options.Expiration,
			LocalCacheExpiration = options.LocalCacheExpiration,
			Flags = options.Flags,
		};
	}

	[SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Internal control-flow exception used only to bypass caching for non-success results.")]
	[SuppressMessage("Design", "CA1064:Exceptions should be public", Justification = "Internal control-flow exception used only inside the cache adapter.")]
	private sealed class StoryblokContentDeliveryApiCacheBypassException<TResponse> : Exception
	{
		public StoryblokContentDeliveryApiCacheBypassException(StoryblokContentDeliveryResult<TResponse> result)
		{
			Result = result;
		}

		public StoryblokContentDeliveryResult<TResponse> Result { get; }
	}
}
