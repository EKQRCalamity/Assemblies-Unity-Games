using UnityEngine.Localization.Metadata;

[Metadata(AllowedTypes = MetadataType.Locale)]
public class FinishedMeta : IMetadata
{
	public bool isFinished = true;

	public bool excludeFromSelection;
}
