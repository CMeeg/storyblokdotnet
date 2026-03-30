using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryHttpClient
{
	internal const string CvCacheTag = "sbcd-internal-cv";

	private readonly HttpClient httpClient;
	private readonly IStoryblokContentDeliveryApiCache cache;

	public StoryblokContentDeliveryHttpClientOptions Options { get; }

	public Uri BaseAddress => httpClient.BaseAddress!;

	public StoryblokContentDeliveryHttpClient(
		HttpClient httpClient,
		StoryblokContentDeliveryHttpClientOptions options,
		IStoryblokContentDeliveryApiCache? cache = null)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(httpClient.BaseAddress);
		ArgumentNullException.ThrowIfNull(options);

		this.httpClient = httpClient;
		this.cache = cache ?? StoryblokContentDeliveryNoOpApiCache.Instance;

		Options = options;
	}

	public async Task<StoryblokContentDeliveryResult<TResponse>> Get<TResponse>(
		StoryblokContentDeliveryRequest request,
		StoryblokContentDeliveryCacheEntryOptions? cacheEntryOptions = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		StoryblokContentDeliveryRequest resolvedRequest = await ResolveRequest(request, cancellationToken).ConfigureAwait(false);

		return await cache
			.GetOrCreate(
				Options.Region,
				resolvedRequest,
				cancel => SendGet<TResponse>(resolvedRequest, cancel),
				cacheEntryOptions,
				cancellationToken)
			.ConfigureAwait(false);
	}

	internal async Task Clear(
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		StoryblokContentDeliveryRequest resolvedRequest = await ResolveRequest(request, cancellationToken).ConfigureAwait(false);
		await cache.Clear(Options.Region, resolvedRequest, cancellationToken).ConfigureAwait(false);
	}

	internal Task ClearByTag(string tag, CancellationToken cancellationToken = default)
	{
		return cache.ClearByTag(tag, cancellationToken);
	}

	internal Task ClearAll(CancellationToken cancellationToken = default)
	{
		return cache.ClearAll(cancellationToken);
	}

	private async Task<StoryblokContentDeliveryResult<TResponse>> SendGet<TResponse>(
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken)
	{
		Uri requestUri = BuildRequestUri(request);
		using HttpRequestMessage httpRequest = new(HttpMethod.Get, requestUri);

		try
		{
			using HttpResponseMessage response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
			{
				StoryblokContentDeliveryError responseError = await BuildErrorFromResponse(response).ConfigureAwait(false);
				return StoryblokContentDeliveryResult<TResponse>.Failure(responseError);
			}

			TResponse? responseBody;
			try
			{
				responseBody = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is JsonException or NotSupportedException)
			{
				return StoryblokContentDeliveryResult<TResponse>.Failure(new StoryblokContentDeliveryError
				{
					StatusCode = response.StatusCode,
					Category = StoryblokContentDeliveryErrorCategory.Serialization,
					Message = "Unable to deserialize the Storyblok response body.",
					Details = exception.Message,
				});
			}

			if (responseBody is null)
			{
				return StoryblokContentDeliveryResult<TResponse>.Failure(new StoryblokContentDeliveryError
				{
					StatusCode = response.StatusCode,
					Category = StoryblokContentDeliveryErrorCategory.Serialization,
					Message = "Storyblok returned an empty response body.",
				});
			}

			return StoryblokContentDeliveryResult<TResponse>.Success(responseBody);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (OperationCanceledException exception)
		{
			return StoryblokContentDeliveryResult<TResponse>.Failure(new StoryblokContentDeliveryError
			{
				Category = StoryblokContentDeliveryErrorCategory.Timeout,
				Message = "The Storyblok request timed out.",
				Details = exception.Message,
			});
		}
		catch (HttpRequestException exception)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}

			if (IsTimeoutException(exception))
			{
				return StoryblokContentDeliveryResult<TResponse>.Failure(new StoryblokContentDeliveryError
				{
					Category = StoryblokContentDeliveryErrorCategory.Timeout,
					Message = "The Storyblok request timed out.",
					Details = exception.Message,
				});
			}

			return StoryblokContentDeliveryResult<TResponse>.Failure(new StoryblokContentDeliveryError
			{
				Category = StoryblokContentDeliveryErrorCategory.Network,
				Message = "The Storyblok request failed due to a network error.",
				Details = exception.Message,
			});
		}
	}

	private static bool IsTimeoutException(HttpRequestException exception)
	{
		return exception.InnerException is OperationCanceledException;
	}

	private async Task<StoryblokContentDeliveryRequest> ResolveRequest(
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken)
	{
		string? resolvedToken = ResolveToken(request);
		long? resolvedCv = request.Query.Cv;

		if (resolvedCv is null && !IsRetrieveCurrentSpacePath(request.Path))
		{
			try
			{
				resolvedCv = await GetCurrentSpaceVersion(cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				resolvedCv = null;
			}
			catch (HttpRequestException)
			{
				resolvedCv = null;
			}
			catch (JsonException)
			{
				resolvedCv = null;
			}
			catch (NotSupportedException)
			{
				resolvedCv = null;
			}
			catch (InvalidOperationException)
			{
				resolvedCv = null;
			}
		}

		return CreateResolvedRequest(request, resolvedToken, resolvedCv);
	}

	private async Task<long> GetCurrentSpaceVersion(
		CancellationToken cancellationToken)
	{
		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response;
		if (Options.Cache.CvTtl <= 0)
		{
			response = await SendGet<RetrieveCurrentSpaceResponse>(new RetrieveCurrentSpaceRequest(new RetrieveCurrentSpaceQuery()), cancellationToken).ConfigureAwait(false);
		}
		else
		{
			StoryblokContentDeliveryCacheEntryOptions cvCacheEntryOptions = BuildCvCacheEntryOptions();
			StoryblokContentDeliverySpacesApi spacesApi = new(this);
			response = await spacesApi
				.RetrieveCurrentSpace(new RetrieveCurrentSpaceQuery(), cvCacheEntryOptions, cancellationToken)
				.ConfigureAwait(false);
		}

		if (!response.IsSuccess)
		{
			throw new InvalidOperationException(response.Error?.Message ?? "Unable to retrieve Storyblok cache version from current space endpoint.");
		}

		return response.Data.Space.Version;
	}

	private StoryblokContentDeliveryCacheEntryOptions BuildCvCacheEntryOptions()
	{
		StoryblokContentDeliveryCacheEntryOptions cacheEntryOptions = new()
		{
			Expiration = TimeSpan.FromSeconds(Options.Cache.CvTtl),
		};
		cacheEntryOptions.Tags.Add(CvCacheTag);

		return cacheEntryOptions;
	}

	private static bool IsRetrieveCurrentSpacePath(string path)
	{
		return string.Equals(path?.Trim(), RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(path?.TrimStart('/').Trim(), RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
	}

	private string? ResolveToken(StoryblokContentDeliveryRequest request)
	{
		foreach (KeyValuePair<string, string?> parameter in request.Query.GetParameters())
		{
			if (string.Equals(parameter.Key, "token", StringComparison.OrdinalIgnoreCase)
				&& !string.IsNullOrWhiteSpace(parameter.Value))
			{
				return parameter.Value.Trim();
			}
		}

		return string.IsNullOrWhiteSpace(Options.Token) ? null : Options.Token;
	}

	private static StoryblokContentDeliveryRequest CreateResolvedRequest(
		StoryblokContentDeliveryRequest request,
		string? resolvedToken,
		long? resolvedCv)
	{
		List<KeyValuePair<string, string?>> parameters = [];

		foreach (KeyValuePair<string, string?> parameter in request.Query.GetParameters())
		{
			if (string.IsNullOrWhiteSpace(parameter.Value)
				|| string.Equals(parameter.Key, "token", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(parameter.Key, "cv", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			parameters.Add(parameter);
		}

		if (!string.IsNullOrWhiteSpace(resolvedToken))
		{
			parameters.Add(new KeyValuePair<string, string?>("token", resolvedToken));
		}

		if (resolvedCv is long finalCv)
		{
			parameters.Add(new KeyValuePair<string, string?>("cv", finalCv.ToString(System.Globalization.CultureInfo.InvariantCulture)));
		}

		return new StoryblokContentDeliveryRequest(request.Path, new ResolvedStoryblokContentDeliveryQuery(parameters));
	}

	private Uri BuildRequestUri(StoryblokContentDeliveryRequest request)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Path);
		ArgumentNullException.ThrowIfNull(request.Query);

		QueryBuilder queryBuilder = new();

		foreach (KeyValuePair<string, string?> parameter in request.Query.GetParameters())
		{
			if (string.IsNullOrWhiteSpace(parameter.Value))
			{
				continue;
			}

			queryBuilder.Add(parameter.Key, parameter.Value);
		}

		UriBuilder requestUriBuilder = new(httpClient.BaseAddress!)
		{
			Path = $"{httpClient.BaseAddress!.AbsolutePath.TrimEnd('/')}/{request.Path.TrimStart('/')}",
			Query = queryBuilder.ToQueryString().Value,
		};

		return requestUriBuilder.Uri;
	}

	private static async Task<StoryblokContentDeliveryError> BuildErrorFromResponse(HttpResponseMessage response)
	{
		string message = $"Storyblok request failed with status code {(int)response.StatusCode}.";
		string? details = null;
		string? responseContent = response.Content is null
			? null
			: await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		if (!string.IsNullOrWhiteSpace(responseContent))
		{
			details = responseContent;
			if (response.Content?.Headers.ContentType?.MediaType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
			{
				if (TryReadMessageFromErrorJson(responseContent, out string? extractedMessage))
				{
					message = extractedMessage!;
				}
			}
		}

		return new StoryblokContentDeliveryError
		{
			StatusCode = response.StatusCode,
			Category = MapErrorCategory(response.StatusCode),
			Message = message,
			Details = details,
			RetryAfter = GetRetryAfter(response),
		};
	}

	private static bool TryReadMessageFromErrorJson(string json, out string? message)
	{
		message = null;

		try
		{
			using JsonDocument document = JsonDocument.Parse(json);
			JsonElement root = document.RootElement;
			if (root.ValueKind == JsonValueKind.Object)
			{
				if (TryReadStringProperty(root, "message", out message)
					|| TryReadStringProperty(root, "error", out message))
				{
					return !string.IsNullOrWhiteSpace(message);
				}
			}
		}
		catch (JsonException)
		{
			return false;
		}

		return false;
	}

	private static bool TryReadStringProperty(JsonElement element, string propertyName, out string? value)
	{
		value = null;
		if (!element.TryGetProperty(propertyName, out JsonElement property)
			|| property.ValueKind != JsonValueKind.String)
		{
			return false;
		}

		value = property.GetString();
		return !string.IsNullOrWhiteSpace(value);
	}

	private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
	{
		if (response.Headers.RetryAfter is null)
		{
			return null;
		}

		DateTimeOffset utcNow = DateTimeOffset.UtcNow;
		if (response.Headers.RetryAfter.Delta is TimeSpan delta)
		{
			return delta > TimeSpan.Zero ? delta : null;
		}

		if (response.Headers.RetryAfter.Date is DateTimeOffset retryAfterDate)
		{
			TimeSpan retryAfter = retryAfterDate - utcNow;
			return retryAfter > TimeSpan.Zero ? retryAfter : null;
		}

		return null;
	}

	private static StoryblokContentDeliveryErrorCategory MapErrorCategory(HttpStatusCode statusCode)
	{
		return statusCode switch
		{
			HttpStatusCode.BadRequest => StoryblokContentDeliveryErrorCategory.BadRequest,
			HttpStatusCode.Unauthorized => StoryblokContentDeliveryErrorCategory.Unauthorized,
			HttpStatusCode.NotFound => StoryblokContentDeliveryErrorCategory.NotFound,
			HttpStatusCode.UnprocessableEntity => StoryblokContentDeliveryErrorCategory.Validation,
			HttpStatusCode.TooManyRequests => StoryblokContentDeliveryErrorCategory.RateLimited,
			HttpStatusCode.InternalServerError
				or HttpStatusCode.BadGateway
				or HttpStatusCode.ServiceUnavailable
				or HttpStatusCode.GatewayTimeout => StoryblokContentDeliveryErrorCategory.ServerError,
			_ => StoryblokContentDeliveryErrorCategory.Unknown,
		};
	}

	private sealed class ResolvedStoryblokContentDeliveryQuery : StoryblokContentDeliveryQuery
	{
		private readonly IReadOnlyList<KeyValuePair<string, string?>> parameters;

		public ResolvedStoryblokContentDeliveryQuery(IReadOnlyList<KeyValuePair<string, string?>> parameters)
		{
			this.parameters = parameters;
		}

		public override IEnumerable<KeyValuePair<string, string?>> GetParameters()
		{
			return parameters;
		}
	}
}
