using System;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.SharedTableData)]
public abstract class ShouldTranslateMeta : IMetadata
{
	public abstract bool ShouldTranslate(LocalizedStringData.TableEntryId id);
}
