using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;

namespace StoryblokDotNet.ContentDeliveryApi.Http;

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

	public async Task<StoryblokContentDeliveryResult<TResponse>> Get<TResponse>(
		string path,
		StoryblokContentDeliveryQuery query,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(query);

		Uri requestUri = BuildRequestUri(path, query);
		using HttpRequestMessage request = new(HttpMethod.Get, requestUri);

		try
		{
			using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

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

	private Uri BuildRequestUri(string path, StoryblokContentDeliveryQuery query)
	{
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
