using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Extensions;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryHttpClient
{
	private readonly HttpClient httpClient;

	public StoryblokContentDeliveryHttpClientOptions Options { get; }

	public Uri BaseAddress => httpClient.BaseAddress!;

	public StoryblokContentDeliveryHttpClient(
		HttpClient httpClient,
		StoryblokContentDeliveryHttpClientOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(httpClient.BaseAddress);

		StoryblokContentDeliveryHttpClientOptions resolvedOptions = options ?? new StoryblokContentDeliveryHttpClientOptions();

		this.httpClient = httpClient;

		Options = resolvedOptions;
	}

	public Task<TResponse?> Get<TResponse>(string path, StoryblokContentDeliveryQuery query)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(query);

		QueryBuilder queryBuilder = new();
		bool hasTokenParameter = false;

		foreach (KeyValuePair<string, string?> parameter in query.GetParameters())
		{
			if (string.IsNullOrWhiteSpace(parameter.Value))
			{
				continue;
			}

			queryBuilder.Add(parameter.Key, parameter.Value);

			if (string.Equals(parameter.Key, "token", StringComparison.OrdinalIgnoreCase))
			{
				hasTokenParameter = true;
			}
		}

		if (!hasTokenParameter && !string.IsNullOrWhiteSpace(Options.Token))
		{
			queryBuilder.Add("token", Options.Token);
		}

		UriBuilder requestUriBuilder = new(httpClient.BaseAddress!)
		{
			Path = $"{httpClient.BaseAddress!.AbsolutePath.TrimEnd('/')}/{path.TrimStart('/')}",
			Query = queryBuilder.ToQueryString().Value,
		};

		return httpClient.GetFromJsonAsync<TResponse>(requestUriBuilder.Uri);
	}
}
