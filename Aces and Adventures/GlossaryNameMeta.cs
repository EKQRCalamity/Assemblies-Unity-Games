using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = (MetadataType.SharedTableData | MetadataType.StringTable | MetadataType.StringTableEntry))]
public class GlossaryNameMeta : IMetadata
{
	[SerializeField]
	protected string _name;

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}
}
