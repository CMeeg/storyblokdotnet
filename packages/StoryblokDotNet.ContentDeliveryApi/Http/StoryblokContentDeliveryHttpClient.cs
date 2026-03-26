using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

public sealed class StoryblokContentDeliveryHttpClient
{
	private readonly HttpClient httpClient;
	private readonly IStoryblokContentDeliveryCvCache cvCache;

	public StoryblokContentDeliveryHttpClientOptions Options { get; }

	public Uri BaseAddress => httpClient.BaseAddress!;

	public StoryblokContentDeliveryHttpClient(
		HttpClient httpClient,
		StoryblokContentDeliveryHttpClientOptions options,
		IStoryblokContentDeliveryCvCache? cvCache = null)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(httpClient.BaseAddress);
		ArgumentNullException.ThrowIfNull(options);

		this.httpClient = httpClient;
		this.cvCache = cvCache ?? StoryblokContentDeliveryNoOpCvCache.Instance;

		Options = options;
	}

	public async Task<StoryblokContentDeliveryResult<TResponse>> Get<TResponse>(
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);

		Uri requestUri = await ResolveRequestUriWithCv(request, cancellationToken).ConfigureAwait(false);
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

	private async Task<Uri> ResolveRequestUriWithCv(
		StoryblokContentDeliveryRequest request,
		CancellationToken cancellationToken)
	{
		if (request.Query.Cv is long queryCv)
		{
			return BuildRequestUri(request, queryCv);
		}

		if (IsRetrieveCurrentSpacePath(request.Path))
		{
			return BuildRequestUri(request);
		}

		try
		{
			long resolvedCv = await cvCache
				.GetOrCreateCv(Options.Region, cancel => GetCurrentSpaceVersion(cancel), cancellationToken)
				.ConfigureAwait(false);
			return BuildRequestUri(request, resolvedCv);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
		{
			return BuildRequestUri(request);
		}
		catch (HttpRequestException)
		{
			return BuildRequestUri(request);
		}
		catch (JsonException)
		{
			return BuildRequestUri(request);
		}
		catch (NotSupportedException)
		{
			return BuildRequestUri(request);
		}
		catch (InvalidOperationException)
		{
			return BuildRequestUri(request);
		}
	}

	private async Task<long> GetCurrentSpaceVersion(CancellationToken cancellationToken)
	{
		StoryblokContentDeliverySpacesApi spacesApi = new(this);
		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await spacesApi
			.RetrieveCurrentSpace(new RetrieveCurrentSpaceQuery(), cancellationToken)
			.ConfigureAwait(false);

		if (!response.IsSuccess)
		{
			throw new InvalidOperationException(response.Error?.Message ?? "Unable to retrieve Storyblok cache version from current space endpoint.");
		}

		return response.Data.Space.Version;
	}

	private static bool IsRetrieveCurrentSpacePath(string path)
	{
		return string.Equals(path?.Trim(), RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(path?.TrimStart('/').Trim(), RetrieveCurrentSpaceRequest.RetrieveCurrentSpacePath.TrimStart('/'), StringComparison.OrdinalIgnoreCase);
	}

	private Uri BuildRequestUri(StoryblokContentDeliveryRequest request, long? overrideCv = null)
	{
		ArgumentNullException.ThrowIfNull(request);
		ArgumentException.ThrowIfNullOrWhiteSpace(request.Path);
		ArgumentNullException.ThrowIfNull(request.Query);

		QueryBuilder queryBuilder = new();
		bool hasTokenParameter = false;
		bool hasCvParameter = false;

		foreach (KeyValuePair<string, string?> parameter in request.Query.GetParameters())
		{
			if (string.IsNullOrWhiteSpace(parameter.Value))
			{
				continue;
			}

			if (string.Equals(parameter.Key, "cv", StringComparison.OrdinalIgnoreCase) && overrideCv is long forcedCv)
			{
				queryBuilder.Add("cv", forcedCv.ToString(System.Globalization.CultureInfo.InvariantCulture));
				hasCvParameter = true;
				continue;
			}

			queryBuilder.Add(parameter.Key, parameter.Value);

			if (string.Equals(parameter.Key, "token", StringComparison.OrdinalIgnoreCase))
			{
				hasTokenParameter = true;
			}

			if (string.Equals(parameter.Key, "cv", StringComparison.OrdinalIgnoreCase))
			{
				hasCvParameter = true;
			}
		}

		if (!hasTokenParameter && !string.IsNullOrWhiteSpace(Options.Token))
		{
			queryBuilder.Add("token", Options.Token);
		}

		if (!hasCvParameter && overrideCv is long finalCv)
		{
			queryBuilder.Add("cv", finalCv.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
}
