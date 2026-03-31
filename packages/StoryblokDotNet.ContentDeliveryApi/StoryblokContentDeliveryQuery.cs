namespace StoryblokDotNet.ContentDeliveryApi;

public class StoryblokContentDeliveryQuery
{
	public string Token { get; set; } = string.Empty;

	public long? Cv { get; set; }

	public virtual IEnumerable<KeyValuePair<string, string?>> GetParameters()
	{
		if (!string.IsNullOrWhiteSpace(Token))
		{
			yield return new KeyValuePair<string, string?>("token", Token);
		}

		if (Cv is long resolvedCv)
		{
			yield return new KeyValuePair<string, string?>("cv", resolvedCv.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}
	}
}
