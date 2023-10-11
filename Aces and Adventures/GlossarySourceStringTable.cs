using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[Serializable]
public class GlossarySourceStringTable : GlossarySource
{
	[HideInInspector]
	[SerializeField]
	public string tableName = "Glossary";

	[SerializeField]
	protected GlossaryTags _requiredTags;

	public static IEnumerable<Term> GetTermsForEntry(StringTableEntry entry, string sourceValue, string targetValue)
	{
		GlossaryEntryMeta glossaryEntryMeta = entry.GlossaryMeta();
		bool verbatim = glossaryEntryMeta?.verbatim ?? false;
		bool properNoun = glossaryEntryMeta?.properNoun ?? false;
		ProtectedTermWhen protectedTermWhen = glossaryEntryMeta?.protectedTermWhen ?? ProtectedTermWhen.Verbatim;
		yield return new Term(sourceValue, targetValue, verbatim, properNoun, protectedTermWhen);
		IncludeAlternateCasing includeAlternateCasing = entry.IncludeAlternatingCasingsInGlossary();
		if (includeAlternateCasing != 0)
		{
			if (includeAlternateCasing == IncludeAlternateCasing.All || sourceValue.GetCasing() == StringCase.Lower)
			{
				yield return new Term(sourceValue.AlternateCasing(), targetValue.AlternateCasing(), verbatim, properNoun, protectedTermWhen);
			}
			if (sourceValue.GetCasing() == StringCase.Title)
			{
				yield return new Term(sourceValue.ToLower().FirstUpper(), targetValue.ToLower().FirstUpper(), verbatim, properNoun, protectedTermWhen);
			}
		}
	}

	public GlossarySourceStringTable()
	{
	}

	public GlossarySourceStringTable(GlossaryTags requiredTags)
	{
		_requiredTags = requiredTags;
	}

	public override async IAsyncEnumerable<Term> GetTermsAsync(Locale locale)
	{
		StringTable sourceTable = LocalizationSettings.StringDatabase.GetTable(tableName, LocalizationUtil.ProjectLocale);
		if (!sourceTable)
		{
			yield break;
		}
		StringTable table = LocalizationSettings.StringDatabase.GetTable(tableName, locale);
		if (!table)
		{
			yield break;
		}
		bool checkTag = EnumUtil.FlagCount(_requiredTags) != 0;
		bool isDefaultGlossary = !checkTag;
		await LocalizationUtil.TranslateTableAsync(table, useGlossary: false);
		foreach (StringTableEntry value in table.Values)
		{
			if (!sourceTable.ContainsKey(value.KeyId) || (checkTag && !value.HasGlossaryTags(_requiredTags)) || (isDefaultGlossary && value.IsExcludedFromDefaultGlossary()))
			{
				continue;
			}
			foreach (Term item in GetTermsForEntry(value, sourceTable[value.KeyId].Value, value.Value))
			{
				yield return item;
			}
		}
	}
}
