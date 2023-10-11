using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.StringTableEntry)]
public class TranslationModeMeta : IMetadataEnum<TranslationMode>, IMetadata
{
	[SerializeField]
	protected TranslationMode _mode;

	public string pendingTranslation
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public string lastTranslationSource
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public bool translationIsOutOfDate
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public TranslationMode lastUsedMode
	{
		get
		{
			return TranslationMode.Auto;
		}
		set
		{
		}
	}

	public string reverseTranslation => null;

	public int translationScore => 0;

	public string lastPulledSource
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public TranslationMode value
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public IEnumerable<string> GetTranslationSources()
	{
		yield return pendingTranslation;
		if (lastUsedMode.IsContextual())
		{
			yield return pendingTranslation.Replace("<u>", "").Replace("</u>", "");
		}
	}
}
