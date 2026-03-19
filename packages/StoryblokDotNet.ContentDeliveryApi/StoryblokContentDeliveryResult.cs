namespace StoryblokDotNet.ContentDeliveryApi;

public sealed class StoryblokContentDeliveryResult<TResponse>
{
	private readonly TResponse? data;

	public TResponse Data => Error is null
		? data!
		: throw new InvalidOperationException("Data is only available for successful results.");

	public StoryblokContentDeliveryError? Error { get; }

	public bool IsSuccess => Error is null;

	private StoryblokContentDeliveryResult(TResponse? data, StoryblokContentDeliveryError? error)
	{
		this.data = data;
		Error = error;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are a standard pattern for result types.")]
	public static StoryblokContentDeliveryResult<TResponse> Success(TResponse data)
	{
		ArgumentNullException.ThrowIfNull(data);

		return new StoryblokContentDeliveryResult<TResponse>(data, null);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are a standard pattern for result types.")]
	public static StoryblokContentDeliveryResult<TResponse> Failure(StoryblokContentDeliveryError error)
	{
		ArgumentNullException.ThrowIfNull(error);

		return new StoryblokContentDeliveryResult<TResponse>(default, error);
	}
}
