using System;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

[Serializable]
[Metadata(AllowedTypes = MetadataType.SharedTableData)]
public abstract class GenerateEntryMeta : IMetadata
{
	protected abstract void _GenerateEntries(StringTable table);

	public void GenerateEntries(StringTable table)
	{
		int count = table.Count;
		_GenerateEntries(table);
		_ = table.Count;
	}
}
