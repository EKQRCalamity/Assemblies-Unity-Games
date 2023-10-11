using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = (MetadataType.SharedTableData | MetadataType.SharedStringTableEntry))]
public class UsesManualTranslationMeta : IMetadata
{
	[SerializeField]
	protected bool _usesManualTranslation;

	public bool usesManualTranslation => _usesManualTranslation;
}
