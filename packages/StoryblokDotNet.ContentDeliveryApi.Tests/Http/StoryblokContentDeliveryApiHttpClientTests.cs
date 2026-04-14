using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Caching;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Http;

public sealed class StoryblokContentDeliveryApiHttpClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClient_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiHttpClient(null!, new StoryblokContentDeliveryApiHttpClientOptions()));
	}

	[Fact]
	public void Constructor_WithHttpClientWithoutBaseAddress_ThrowsArgumentNullException()
	{
		using HttpClient httpClient = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiHttpClient(httpClient, new StoryblokContentDeliveryApiHttpClientOptions()));
	}

	[Fact]
	public void Constructor_WithoutOptions_ThrowsArgumentNullException()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryApiHttpClient(httpClient, null!));
	}

	[Fact]
	public void Constructor_WithExplicitOptions_UsesProvidedOptions()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClientOptions options = new()
		{
			Region = StoryblokRegion.China,
			Token = "my-token",
		};

		StoryblokContentDeliveryApiHttpClient client = new(httpClient, options);

		Assert.Same(options, client.Options);
		Assert.Equal(StoryblokRegion.China, client.Options.Region);
		Assert.Equal("my-token", client.Options.Token);
	}

	[Fact]
	public async Task Get_WithSerializedQuery_UsesBasePathAndQueryString()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		RetrieveCurrentSpaceQuery query = new()
		{
			Token = "my token",
		};
		StoryblokContentDeliveryRequest request = new("/spaces/me", query);

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=my%20token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
	}

	[Fact]
	public async Task Get_WithEmptyQueryToken_UsesClientToken()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClientOptions options = new()
		{
			Token = "configured-token",
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, options);
		RetrieveCurrentSpaceQuery query = new();
		StoryblokContentDeliveryRequest request = new("/spaces/me", query);

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=configured-token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
	}

	[Fact]
	public async Task Get_WithNonNullQueryValues_EmitsAllValuesIncludingEmptyStrings()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = string.Empty,
		});
		StoryblokContentDeliveryRequest request = new("/spaces/me", new ParameterPassthroughQuery(
			new KeyValuePair<string, string?>("token", string.Empty),
			new KeyValuePair<string, string?>("draft", string.Empty),
			new KeyValuePair<string, string?>("name", "storyblok"),
			new KeyValuePair<string, string?>("skip", null)));

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.NotNull(handler.RequestUri);
		Assert.Contains("token=", handler.RequestUri!.Query, StringComparison.Ordinal);
		Assert.Contains("draft=", handler.RequestUri.Query, StringComparison.Ordinal);
		Assert.Contains("name=storyblok", handler.RequestUri.Query, StringComparison.Ordinal);
		Assert.DoesNotContain("skip", handler.RequestUri.Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Get_WithoutCv_ResolvesCvFromCurrentSpaceAndAppendsCvParameter()
	{
		using RecordingHttpMessageHandler handler = new(request =>
		{
			if (request.RequestUri?.AbsolutePath.EndsWith("/spaces/me", StringComparison.OrdinalIgnoreCase) == true)
			{
				return CreateJsonResponse("""
				{
				  "space": {
				    "version": 1735815318
				  }
				}
				""");
			}

			return CreateJsonResponse("{}");
		});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryRequest request = new("/stories", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Equal(2, handler.RequestUris.Count);
		Assert.EndsWith("/spaces/me?token=configured-token", handler.RequestUris[0].AbsoluteUri, StringComparison.Ordinal);
		Assert.Equal("/v2/cdn/stories", handler.RequestUris[1].AbsolutePath);
		Assert.Contains("token=configured-token", handler.RequestUris[1].Query, StringComparison.Ordinal);
		Assert.Contains("cv=1735815318", handler.RequestUris[1].Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Get_WithoutCv_UsesInternalCvCacheTagWhenResolvingCurrentSpace()
	{
		using RecordingHttpMessageHandler handler = new(request =>
		{
			if (request.RequestUri?.AbsolutePath.EndsWith("/spaces/me", StringComparison.OrdinalIgnoreCase) == true)
			{
				return CreateJsonResponse("""
				{
				  "space": {
				    "version": 1735815318
				  }
				}
				""");
			}

			return CreateJsonResponse("{}");
		});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		RecordingApiCache cache = new();
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		}, cache);
		StoryblokContentDeliveryRequest request = new("/stories", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Equal(2, cache.CacheCalls.Count);
		Assert.Contains(cache.CacheCalls, call => call.Options?.Tags.Contains(StoryblokContentDeliveryApiHttpClient.CvCacheTag) == true);
	}

	[Fact]
	public async Task Get_WithoutCvAndCurrentSpaceResolutionFails_ContinuesWithoutCvParameter()
	{
		using RecordingHttpMessageHandler handler = new(request =>
		{
			if (request.RequestUri?.AbsolutePath.EndsWith("/spaces/me", StringComparison.OrdinalIgnoreCase) == true)
			{
				return new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("{\"message\":\"failed\"}", Encoding.UTF8, "application/json"),
				};
			}

			return CreateJsonResponse("{}");
		});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryRequest request = new("/stories", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Equal(2, handler.RequestUris.Count);
		Assert.EndsWith("/spaces/me?token=configured-token", handler.RequestUris[0].AbsoluteUri, StringComparison.Ordinal);
		Assert.EndsWith("/stories?token=configured-token", handler.RequestUris[1].AbsoluteUri, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Get_WithExplicitCv_SkipsCurrentSpaceResolution()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		RetrieveCurrentSpaceQuery query = new()
		{
			Cv = 1735815318,
		};
		StoryblokContentDeliveryRequest request = new("/stories", query);

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Single(handler.RequestUris);
		Assert.Equal("/v2/cdn/stories", handler.RequestUris[0].AbsolutePath);
		Assert.Contains("token=configured-token", handler.RequestUris[0].Query, StringComparison.Ordinal);
		Assert.Contains("cv=1735815318", handler.RequestUris[0].Query, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Get_WithMissingCvForCurrentSpacePath_SkipsCurrentSpaceResolution()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions
		{
			Token = "configured-token",
		});
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(response.IsSuccess);
		Assert.Single(handler.RequestUris);
		Assert.EndsWith("/spaces/me?token=configured-token", handler.RequestUris[0].AbsoluteUri, StringComparison.Ordinal);
	}

	[Fact]
	public async Task Get_WithUnauthorizedResponse_ReturnsUnauthorizedError()
	{
		using RecordingHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
		{
			Content = new StringContent("{\"message\":\"Unauthorized\"}", Encoding.UTF8, "application/json"),
		});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.False(response.IsSuccess);
		Assert.NotNull(response.Error);
		Assert.Equal(HttpStatusCode.Unauthorized, response.Error!.StatusCode);
		Assert.Equal(StoryblokContentDeliveryErrorCategory.Unauthorized, response.Error.Category);
		Assert.Equal("Unauthorized", response.Error.Message);
	}

	[Fact]
	public async Task Get_WithRateLimitResponse_ReturnsRetryAfter()
	{
		using RecordingHttpMessageHandler handler = new(_ =>
		{
			HttpResponseMessage response = new(HttpStatusCode.TooManyRequests)
			{
				Content = new StringContent("{\"message\":\"Too many requests\"}", Encoding.UTF8, "application/json"),
			};
			response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(2));
			return response;
		});
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.False(response.IsSuccess);
		Assert.NotNull(response.Error);
		Assert.Equal(HttpStatusCode.TooManyRequests, response.Error!.StatusCode);
		Assert.Equal(StoryblokContentDeliveryErrorCategory.RateLimited, response.Error.Category);
		Assert.Equal(TimeSpan.FromSeconds(2), response.Error.RetryAfter);
	}

	[Fact]
	public async Task Get_WithInvalidJson_ReturnsSerializationError()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("not-json"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await client.Get<RetrieveCurrentSpaceResponse>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.False(response.IsSuccess);
		Assert.NotNull(response.Error);
		Assert.Equal(StoryblokContentDeliveryErrorCategory.Serialization, response.Error!.Category);
	}

	[Fact]
	public async Task Get_WithCanceledToken_ThrowsOperationCanceledException()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("{}"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		using CancellationTokenSource cancellationTokenSource = new();
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());
		await cancellationTokenSource.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			client.Get<object>(request, cancellationToken: cancellationTokenSource.Token));
	}

	[Fact]
	public async Task Get_WithNullJsonPayload_ReturnsSerializationError()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("null"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryApiClient.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryApiHttpClient client = new(httpClient, new StoryblokContentDeliveryApiHttpClientOptions());
		StoryblokContentDeliveryRequest request = new("/spaces/me", new RetrieveCurrentSpaceQuery());

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await client.Get<RetrieveCurrentSpaceResponse>(request, cancellationToken: TestContext.Current.CancellationToken);

		Assert.False(response.IsSuccess);
		Assert.NotNull(response.Error);
		Assert.Equal(StoryblokContentDeliveryErrorCategory.Serialization, response.Error.Category);
	}

	private static HttpResponseMessage CreateJsonResponse(string json)
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json"),
		};
	}

	private sealed class ParameterPassthroughQuery : StoryblokContentDeliveryQuery
	{
		private readonly IReadOnlyList<KeyValuePair<string, string?>> parameters;

		public ParameterPassthroughQuery(params KeyValuePair<string, string?>[] parameters)
		{
			this.parameters = parameters;
		}

		public override IEnumerable<KeyValuePair<string, string?>> GetParameters()
		{
			return parameters;
		}
	}
}
