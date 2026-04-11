namespace StoryblokDotNet.ContentDeliveryApi.Tags;

public sealed class RetrieveMultipleTagsQuery : StoryblokContentDeliveryQuery
{
	public string? StartsWith { get; set; }

	public StoryblokVersion? Version { get; set; }

	public override IEnumerable<KeyValuePair<string, string?>> GetParameters()
	{
		foreach (KeyValuePair<string, string?> parameter in base.GetParameters())
		{
			yield return parameter;
		}

		if (StartsWith is not null)
		{
			yield return new KeyValuePair<string, string?>("starts_with", StartsWith);
		}

		if (Version is StoryblokVersion resolvedVersion)
		{
			string? versionValue = resolvedVersion switch
			{
				StoryblokVersion.Draft => "draft",
				StoryblokVersion.Published => "published",
				_ => null,
			};

			if (versionValue is not null)
			{
				yield return new KeyValuePair<string, string?>("version", versionValue);
			}
		}
	}
}
