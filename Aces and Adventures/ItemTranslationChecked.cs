using System;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.StringTableEntry)]
public class ItemTranslationChecked : IMetadataEnum<ItemTranslationChecked.Checked>, IMetadata
{
	[UIField(tooltip = "Type in Y to indicate that, yes, this automatic translation has been checked and is valid.")]
	public enum Checked
	{
		N,
		Y
	}

	[SerializeField]
	protected Checked _isChecked;

	public Checked value
	{
		get
		{
			return _isChecked;
		}
		set
		{
			_isChecked = value;
		}
	}

	public string lastAutomatedTranslation
	{
		get
		{
			return null;
		}
		set
		{
		}
	}
}
