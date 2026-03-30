using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace StoryblokDotNet.ContentDeliveryApi.Caching;

[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Cache keys are intentionally normalized to lowercase to keep key identity stable across case-sensitive providers.")]
internal static class StoryblokContentDeliveryApiCacheKeyBuilder
{
	private const string CacheKeyPrefix = "sb-cd-api:req";

	public static string Create(StoryblokRegion region, StoryblokContentDeliveryRequest request, int maximumKeyLength)
	{
		ArgumentNullException.ThrowIfNull(request);

		string normalizedRegion = region.ToString().ToLowerInvariant();
		string normalizedPath = NormalizePath(request.Path);
		IReadOnlyList<KeyValuePair<string, string>> parameters = request.Query
			.GetParameters()
			.Where(static parameter => !string.IsNullOrWhiteSpace(parameter.Value))
			.Select(static parameter => new KeyValuePair<string, string>(
				parameter.Key.Trim().ToLowerInvariant(),
				parameter.Value!.Trim().ToLowerInvariant()))
			.OrderBy(static parameter => parameter.Key, StringComparer.Ordinal)
			.ThenBy(static parameter => parameter.Value, StringComparer.Ordinal)
			.ToList();

		StringBuilder canonicalBuilder = new();
		canonicalBuilder.Append("region=");
		canonicalBuilder.Append(normalizedRegion);
		canonicalBuilder.Append("|path=");
		canonicalBuilder.Append(normalizedPath);

		foreach (KeyValuePair<string, string> parameter in parameters)
		{
			canonicalBuilder.Append('|');
			canonicalBuilder.Append(parameter.Key);
			canonicalBuilder.Append('=');

			if (string.Equals(parameter.Key, "token", StringComparison.OrdinalIgnoreCase))
			{
				canonicalBuilder.Append(Hash(parameter.Value));
			}
			else
			{
				canonicalBuilder.Append(Uri.EscapeDataString(parameter.Value).ToLowerInvariant());
			}
		}

		string canonicalKey = $"{CacheKeyPrefix}:{canonicalBuilder}";
		if (canonicalKey.Length <= maximumKeyLength)
		{
			return canonicalKey;
		}

		return $"{CacheKeyPrefix}:{normalizedRegion}:{Hash(canonicalBuilder.ToString())}";
	}

	private static string NormalizePath(string path)
	{
		string trimmedPath = path.Trim();
		if (!trimmedPath.StartsWith('/'))
		{
			trimmedPath = $"/{trimmedPath}";
		}

		return trimmedPath.ToLowerInvariant();
	}

	private static string Hash(string value)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		byte[] hash = SHA256.HashData(bytes);
		return Convert.ToHexString(hash).ToLowerInvariant();
	}
}
