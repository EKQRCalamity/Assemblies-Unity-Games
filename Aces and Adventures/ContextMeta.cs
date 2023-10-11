using System;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = (MetadataType.StringTableEntry | MetadataType.SharedStringTableEntry))]
public class ContextMeta : IMetadata
{
	public string context => null;

	public string aliasContext => null;

	public bool applyGlossaryToContext => false;
}
