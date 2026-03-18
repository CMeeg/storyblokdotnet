namespace StoryblokDotNet.ContentDeliveryApi;

public class StoryblokContentDeliveryQuery
{
	public string Token { get; set; } = string.Empty;

	public virtual IEnumerable<KeyValuePair<string, string?>> GetParameters()
	{
		if (!string.IsNullOrWhiteSpace(Token))
		{
			yield return new KeyValuePair<string, string?>("token", Token);
		}
	}
}
