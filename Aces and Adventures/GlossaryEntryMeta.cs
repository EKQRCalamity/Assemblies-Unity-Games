using System;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = (MetadataType.StringTableEntry | MetadataType.SharedStringTableEntry))]
public class GlossaryEntryMeta : IMetadata
{
	public IncludeAlternateCasing includeAlternateCasing => IncludeAlternateCasing.None;

	public GlossaryTags tags => (GlossaryTags)0;

	public ExcludeFromDefaultGlossary excludeFromDefault => ExcludeFromDefaultGlossary.Never;

	public bool verbatim => false;

	public bool properNoun => false;

	public ProtectedTermWhen protectedTermWhen => ProtectedTermWhen.Verbatim;
}
