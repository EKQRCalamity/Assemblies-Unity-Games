using System;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = (MetadataType.StringTableEntry | MetadataType.SharedStringTableEntry))]
public class ExcludedTermsMeta : IMetadata
{
	public List<LocalizedString> terms => null;
}
