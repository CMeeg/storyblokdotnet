using System.Text.RegularExpressions;
using StoryblokDotNet.ContentDeliveryApi.Caching;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Caching;

public sealed class StoryblokContentDeliveryApiCacheKeyBuilderTests
{
	[Fact]
	public void Create_WithVeryLongCanonicalRequest_UsesHashedFallbackWithinMaximumLength()
	{
		StoryblokContentDeliveryRequest request = new(
			"stories",
			new LongQuery
			{
				Token = "token-value",
			});

		int maximumKeyLength = 512;
		string cacheKey = StoryblokContentDeliveryApiCacheKeyBuilder.Create(StoryblokRegion.Eu, request, maximumKeyLength);

		Assert.True(cacheKey.Length <= maximumKeyLength);
		Assert.Matches(new Regex("^sb-cd-api:req:eu:[0-9a-f]{64}$", RegexOptions.CultureInvariant), cacheKey);
	}

	[Fact]
	public void Create_WithMixedCaseValues_NormalizesCacheKeyToLowercase()
	{
		StoryblokContentDeliveryRequest mixedCaseRequest = new(
			"Stories/Featured",
			new MixedCaseQuery
			{
				Token = "ToKeN-VaLuE",
			});

		StoryblokContentDeliveryRequest lowercaseRequest = new(
			"stories/featured",
			new MixedCaseQuery
			{
				Token = "token-value",
			});

		string mixedCaseKey = StoryblokContentDeliveryApiCacheKeyBuilder.Create(StoryblokRegion.Eu, mixedCaseRequest, 1024);
		string lowercaseKey = StoryblokContentDeliveryApiCacheKeyBuilder.Create(StoryblokRegion.Eu, lowercaseRequest, 1024);

		Assert.Equal(lowercaseKey, mixedCaseKey);
		Assert.DoesNotMatch(new Regex("[A-Z]", RegexOptions.CultureInvariant), mixedCaseKey);
	}

	private sealed class LongQuery : StoryblokContentDeliveryQuery
	{
		private const string LargeValue = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

		public override IEnumerable<KeyValuePair<string, string?>> GetParameters()
		{
			foreach (KeyValuePair<string, string?> parameter in base.GetParameters())
			{
				yield return parameter;
			}

			yield return new KeyValuePair<string, string?>("starts_with", LargeValue);
			yield return new KeyValuePair<string, string?>("excluding_fields", LargeValue);
			yield return new KeyValuePair<string, string?>("with_tag", LargeValue);
			yield return new KeyValuePair<string, string?>("resolve_relations", LargeValue);
		}
	}

	private sealed class MixedCaseQuery : StoryblokContentDeliveryQuery
	{
		public override IEnumerable<KeyValuePair<string, string?>> GetParameters()
		{
			foreach (KeyValuePair<string, string?> parameter in base.GetParameters())
			{
				yield return parameter;
			}

			yield return new KeyValuePair<string, string?>("Starts_With", "Home/Feature");
			yield return new KeyValuePair<string, string?>("With_Tag", "NewsRoom");
		}
	}
}
