using System.Diagnostics.CodeAnalysis;

namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryResult<TResponse>
{
	public TResponse? Data { get; }

	public StoryblokContentDeliveryError? Error { get; }

	[MemberNotNullWhen(true, nameof(Data))]
	[MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => Error is null;

	private StoryblokContentDeliveryResult(TResponse? data, StoryblokContentDeliveryError? error)
	{
		Data = data;
		Error = error;
	}

	[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are a standard pattern for result types.")]
	public static StoryblokContentDeliveryResult<TResponse> Success(TResponse data)
	{
		ArgumentNullException.ThrowIfNull(data);

		return new StoryblokContentDeliveryResult<TResponse>(data, null);
	}

	[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are a standard pattern for result types.")]
	public static StoryblokContentDeliveryResult<TResponse> Failure(StoryblokContentDeliveryError error)
	{
		ArgumentNullException.ThrowIfNull(error);

		return new StoryblokContentDeliveryResult<TResponse>(default, error);
	}
}
