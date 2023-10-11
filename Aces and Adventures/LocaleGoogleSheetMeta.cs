using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Translate.V3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

[Serializable]
[Metadata(AllowedTypes = MetadataType.Locale)]
public class LocaleGoogleSheetMeta : IMetadata
{
	private static readonly Dictionary<string, Func<List<GlossarySource>>> _AutomatedGlossaries;

	[SerializeField]
	[ShowOnly]
	protected string _spreadSheetId;

	[SerializeField]
	protected bool _usesGenderColumn = true;

	[SerializeField]
	protected List<GlossaryData> _glossaries;

	[SerializeField]
	[HideInInspector]
	protected string _linkedSpreadsheetId;

	[SerializeField]
	[HideInInspector]
	protected bool _hasDeletedDefaultSheet;

	[SerializeField]
	protected OutOfDateTranslationHandling _outOfDateTranslationHandling;

	[SerializeField]
	protected bool _forcePush;

	private Dictionary<int, StringTable> _tablesBySheetId;

	public string spreadSheetId
	{
		get
		{
			return _spreadSheetId;
		}
		set
		{
			_spreadSheetId = value;
		}
	}

	public string linkedSpreadsheetId
	{
		get
		{
			return _linkedSpreadsheetId;
		}
		set
		{
			_linkedSpreadsheetId = value;
		}
	}

	public bool hasDeletedDefaultSheet
	{
		get
		{
			return _hasDeletedDefaultSheet;
		}
		set
		{
			_hasDeletedDefaultSheet = value;
		}
	}

	public bool usesGenderColumn => _usesGenderColumn;

	public OutOfDateTranslationHandling? outOfDateTranslationHandling => _outOfDateTranslationHandling.GetValue();

	static LocaleGoogleSheetMeta()
	{
		_AutomatedGlossaries = new Dictionary<string, Func<List<GlossarySource>>>(StringComparer.OrdinalIgnoreCase);
		_AutomatedGlossaries.Add("Default", GlossaryData.CreateDefaultSources);
		_AutomatedGlossaries.Add("Adventure", () => new List<GlossarySource>
		{
			new GlossarySourceStringTable(GlossaryTags.Adventure),
			new GlossarySourceGlobalVariables(),
			new GlossarySourceVariableNames()
		});
		_AutomatedGlossaries.Add("AbilityName", () => new List<GlossarySource>
		{
			new GlossarySourceStringTable(GlossaryTags.AbilityName)
		});
	}

	public GlossaryData GetGlossaryData(string glossaryName)
	{
		if (!glossaryName.HasVisibleCharacter())
		{
			return null;
		}
		GlossaryData glossaryData = _glossaries?.FirstOrDefault((GlossaryData g) => string.Equals(g.name, glossaryName, StringComparison.OrdinalIgnoreCase));
		if (glossaryData != null)
		{
			return glossaryData;
		}
		Func<List<GlossarySource>> valueOrDefault = _AutomatedGlossaries.GetValueOrDefault(glossaryName);
		if (valueOrDefault != null)
		{
			return (_glossaries ?? (_glossaries = new List<GlossaryData>())).AddReturn(new GlossaryData(glossaryName, valueOrDefault()));
		}
		return null;
	}

	public async Task<Glossary> GetGlossaryAsync(Locale locale, string glossaryName)
	{
		GlossaryData glossaryData = GetGlossaryData(glossaryName);
		return (glossaryData == null) ? null : (await glossaryData.GetGlossaryAsync(locale));
	}

	public Dictionary<int, StringTable> GetTablesBySheetId(Locale locale)
	{
		return _tablesBySheetId ?? (_tablesBySheetId = (from table in locale.GetAllStringTables()
			where table.SheetId() != 0
			select table).ToDictionary((StringTable t) => t.SheetId(), (StringTable t) => t));
	}

	public void ClearGlossaryHashes()
	{
		if (_glossaries.IsNullOrEmpty())
		{
			return;
		}
		foreach (GlossaryData glossary in _glossaries)
		{
			glossary.ClearHash();
		}
	}

	public bool ForcePush()
	{
		if (_forcePush)
		{
			return !(_forcePush = false);
		}
		return false;
	}

	public static implicit operator bool(LocaleGoogleSheetMeta meta)
	{
		return !string.IsNullOrWhiteSpace(meta);
	}

	public static implicit operator string(LocaleGoogleSheetMeta meta)
	{
		return meta?._spreadSheetId ?? "";
	}
}
