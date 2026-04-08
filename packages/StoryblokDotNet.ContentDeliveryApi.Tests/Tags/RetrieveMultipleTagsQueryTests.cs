using StoryblokDotNet.ContentDeliveryApi.Tags;

namespace StoryblokDotNet.ContentDeliveryApi.Tests.Tags;

public sealed class RetrieveMultipleTagsQueryTests
{
	[Fact]
	public void Build_WithTokenSetByBuilder_ReturnsQueryWithToken()
	{
		RetrieveMultipleTagsQueryBuilder sut = new();

		RetrieveMultipleTagsQuery result = sut
			.WithToken("preview token")
			.Build();

		Assert.Equal("preview token", result.Token);
	}

	[Fact]
	public void Build_WithCvSetByBuilder_ReturnsQueryWithCv()
	{
		RetrieveMultipleTagsQuery result = new RetrieveMultipleTagsQueryBuilder()
			.WithCv(1735815318)
			.Build();

		Assert.Equal(1735815318, result.Cv);
	}

	[Fact]
	public void Build_WithStartsWithSetByBuilder_ReturnsQueryWithStartsWith()
	{
		RetrieveMultipleTagsQuery result = new RetrieveMultipleTagsQueryBuilder()
			.WithStartsWith("blog/")
			.Build();

		Assert.Equal("blog/", result.StartsWith);
	}

	[Fact]
	public void Build_WithVersionSetByBuilder_ReturnsQueryWithVersion()
	{
		RetrieveMultipleTagsQuery result = new RetrieveMultipleTagsQueryBuilder()
			.WithVersion(StoryblokVersion.Draft)
			.Build();

		Assert.Equal(StoryblokVersion.Draft, result.Version);
	}

	[Fact]
	public void GetParameters_WithToken_ReturnsTokenParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Single(result);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("preview token", result[0].Value);
	}

	[Fact]
	public void GetParameters_WithTokenAndCv_ReturnsTokenAndCvParameters()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
			Cv = 1735815318,
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Equal(2, result.Length);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("preview token", result[0].Value);
		Assert.Equal("cv", result[1].Key);
		Assert.Equal("1735815318", result[1].Value);
	}

	[Fact]
	public void GetParameters_WithStartsWith_ReturnsStartsWithParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
			StartsWith = "blog/",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Equal(2, result.Length);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("starts_with", result[1].Key);
		Assert.Equal("blog/", result[1].Value);
	}

	[Fact]
	public void GetParameters_WithoutStartsWith_OmitsStartsWithParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.DoesNotContain(result, p => p.Key == "starts_with");
	}

	[Fact]
	public void GetParameters_WithDraftVersion_ReturnsDraftVersionParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
			Version = StoryblokVersion.Draft,
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Equal(2, result.Length);
		Assert.Equal("token", result[0].Key);
		Assert.Equal("version", result[1].Key);
		Assert.Equal("draft", result[1].Value);
	}

	[Fact]
	public void GetParameters_WithPublishedVersion_ReturnsPublishedVersionParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
			Version = StoryblokVersion.Published,
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Equal(2, result.Length);
		Assert.Equal("version", result[1].Key);
		Assert.Equal("published", result[1].Value);
	}

	[Fact]
	public void GetParameters_WithoutVersion_OmitsVersionParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.DoesNotContain(result, p => p.Key == "version");
	}

	[Fact]
	public void GetParameters_WithUndefinedVersion_OmitsVersionParameter()
	{
		RetrieveMultipleTagsQuery sut = new()
		{
			Token = "preview token",
			Version = (StoryblokVersion)999,
		};

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.DoesNotContain(result, p => p.Key == "version");
	}

	[Fact]
	public void GetParameters_WithoutToken_ReturnsEmptyList()
	{
		RetrieveMultipleTagsQuery sut = new();

		KeyValuePair<string, string?>[] result = [.. sut.GetParameters()];

		Assert.Empty(result);
	}
}
