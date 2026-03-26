using System.Net;
using System.Text;
using StoryblokDotNet.ContentDeliveryApi.Http;
using StoryblokDotNet.ContentDeliveryApi.Spaces;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Http;

public sealed class StoryblokContentDeliveryHttpClientTests
{
	[Fact]
	public void Constructor_WithoutHttpClient_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClient(null!, new StoryblokContentDeliveryHttpClientOptions()));
	}

	[Fact]
	public void Constructor_WithHttpClientWithoutBaseAddress_ThrowsArgumentNullException()
	{
		using HttpClient httpClient = new();

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClient(httpClient, new StoryblokContentDeliveryHttpClientOptions()));
	}

	[Fact]
	public void Constructor_WithoutOptions_ThrowsArgumentNullException()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};

		Assert.Throws<ArgumentNullException>(() => new StoryblokContentDeliveryHttpClient(httpClient, null!));
	}

	[Fact]
	public void Constructor_WithExplicitOptions_UsesProvidedOptions()
	{
		using HttpClient httpClient = new()
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Region = StoryblokRegion.China,
			Token = "my-token",
		};

		StoryblokContentDeliveryHttpClient client = new(httpClient, options);

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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());
		RetrieveCurrentSpaceQuery query = new()
		{
			Token = "my token",
		};

		StoryblokContentDeliveryResult<object> response = await client.Get<object>("/spaces/me", query, TestContext.Current.CancellationToken);

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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClientOptions options = new()
		{
			Token = "configured-token",
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, options);
		RetrieveCurrentSpaceQuery query = new();

		StoryblokContentDeliveryResult<object> response = await client.Get<object>("/spaces/me", query, TestContext.Current.CancellationToken);

		Assert.NotNull(handler.RequestUri);
		Assert.Equal("https://api.storyblok.com/v2/cdn/spaces/me?token=configured-token", handler.RequestUri!.AbsoluteUri);
		Assert.True(response.IsSuccess);
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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>("/spaces/me", new RetrieveCurrentSpaceQuery(), TestContext.Current.CancellationToken);

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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());

		StoryblokContentDeliveryResult<object> response = await client.Get<object>("/spaces/me", new RetrieveCurrentSpaceQuery(), TestContext.Current.CancellationToken);

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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await client.Get<RetrieveCurrentSpaceResponse>("/spaces/me", new RetrieveCurrentSpaceQuery(), TestContext.Current.CancellationToken);

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
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());
		using CancellationTokenSource cancellationTokenSource = new();
		await cancellationTokenSource.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			client.Get<object>("/spaces/me", new RetrieveCurrentSpaceQuery(), cancellationTokenSource.Token));
	}

	[Fact]
	public async Task Get_WithNullJsonPayload_ReturnsSerializationError()
	{
		using RecordingHttpMessageHandler handler = new(_ => CreateJsonResponse("null"));
		using HttpClient httpClient = new(handler)
		{
			BaseAddress = StoryblokContentDeliveryHttpClientFactory.GetBaseAddress(StoryblokRegion.Eu),
		};
		StoryblokContentDeliveryHttpClient client = new(httpClient, new StoryblokContentDeliveryHttpClientOptions());

		StoryblokContentDeliveryResult<RetrieveCurrentSpaceResponse> response = await client.Get<RetrieveCurrentSpaceResponse>("/spaces/me", new RetrieveCurrentSpaceQuery(), TestContext.Current.CancellationToken);

		Assert.False(response.IsSuccess);
		Assert.NotNull(response.Error);
		Assert.Equal(StoryblokContentDeliveryErrorCategory.Serialization, response.Error!.Category);
		Assert.Throws<InvalidOperationException>(() => _ = response.Data);
	}

	private static HttpResponseMessage CreateJsonResponse(string json)
	{
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json"),
		};
	}
}
