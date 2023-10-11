using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Translate.V3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

[Serializable]
public class GlossaryData
{
	[Serializable]
	public struct Term
	{
		public string source;

		public string target;

		public Term(string source, string target)
		{
			this.source = source;
			this.target = target;
		}
	}

	[SerializeField]
	protected string _name = "Default";

	[SerializeReference]
	protected List<GlossarySource> _sources;

	[SerializeField]
	[Tooltip("Clear this field to force a rebuild of the glossary and its corresponding translations.")]
	protected string _hash;

	[HideInInspector]
	protected Term[] _previousFlexibleTerms;

	protected Task<Glossary> _glossary;

	protected Dictionary<string, string> _flexibleTerms;

	protected Dictionary<string, string> _protectedTerms;

	protected Dictionary<string, string> _properNouns;

	public string name => _name;

	public static List<GlossarySource> CreateDefaultSources()
	{
		return new List<GlossarySource>
		{
			new GlossarySourceStringTable(),
			new GlossarySourceAbilityNames(),
			new GlossarySourceGlobalVariables(),
			new GlossarySourceVariableNames()
		};
	}

	public GlossaryData()
	{
	}

	public GlossaryData(string name, List<GlossarySource> sources)
	{
		_name = name;
		_sources = sources;
	}

	private string _GetGlossaryName(Locale locale)
	{
		return GoogleUtil.Translate.ToGlossaryName(locale.LocaleName + " " + _name);
	}

	private async Task<Glossary> _CreateGlossaryAsync(Locale locale)
	{
		if (_sources.IsNullOrEmpty())
		{
			_sources = CreateDefaultSources();
		}
		string glossaryName = _GetGlossaryName(locale);
		Glossary glossary = GoogleUtil.Translate.NewGlossary(glossaryName, locale.Identifier.Code);
		Dictionary<string, string> glossaryTerms = new Dictionary<string, string>();
		_flexibleTerms = new Dictionary<string, string>();
		_properNouns = new Dictionary<string, string>();
		_protectedTerms = new Dictionary<string, string>();
		foreach (GlossarySource source in _sources)
		{
			await foreach (GlossarySource.Term item in source.GetTermsAsync(locale))
			{
				Dictionary<string, string> dictionary = (item.verbatim ? glossaryTerms : _flexibleTerms);
				if (item.includeInGlossary && !dictionary.ContainsKey(item.source))
				{
					dictionary[item.source] = item.target;
				}
				if (item.properNoun)
				{
					_properNouns[item.source] = item.target;
				}
				if (item.isProtected)
				{
					_protectedTerms[item.source] = item.target;
				}
			}
		}
		if (glossaryTerms.Count == 0)
		{
			glossaryTerms.Add("NULL_TERM", "NULL_TERM");
		}
		Stream csvStream = glossaryTerms.WriteToCsvStream();
		string hash = csvStream.GetHash128String();
		bool flag = _hash != hash;
		bool hadPreviousHash = _hash.HasVisibleCharacter();
		_hash = hash;
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>(_flexibleTerms);
		if (!_previousFlexibleTerms.IsNullOrEmpty())
		{
			foreach (KeyValuePair<string, string> item2 in _previousFlexibleTerms.ToDictionary((Term t) => t.source, (Term t) => t.target))
			{
				if (hadPreviousHash && dictionary2.GetValueOrDefault(item2.Key) == item2.Value)
				{
					dictionary2.Remove(item2.Key);
				}
				else
				{
					dictionary2[item2.Key] = item2.Value;
				}
			}
		}
		bool flag2 = flag;
		if (!flag2)
		{
			flag2 = await GoogleUtil.Translate.TryGetGlossaryAsync(glossary) == null;
		}
		if (flag2)
		{
			Debug.Log($"[{locale}] is uploading [{_name}] glossary with hash: [{hash}]");
			string glossaryPath = GoogleUtil.Translate.ToGlossaryPath(glossaryName);
			if (await GoogleUtil.Storage.FileExistsAsync(glossaryPath))
			{
				foreach (KeyValuePair<string, string> item3 in GoogleUtil.CsvStreamToDictionary(await GoogleUtil.Storage.DownloadAsync(glossaryPath, new MemoryStream())))
				{
					if (hadPreviousHash && glossaryTerms.GetValueOrDefault(item3.Key) == item3.Value)
					{
						glossaryTerms.Remove(item3.Key);
					}
					else
					{
						glossaryTerms[item3.Key] = item3.Value;
					}
				}
			}
			await GoogleUtil.Translate.UploadGlossaryCsvAsync(glossaryName, csvStream);
			await GoogleUtil.Translate.UpdateGlossaryAsync(glossary);
		}
		else
		{
			glossaryTerms.Clear();
		}
		HashSet<string> glossaryTermsThatChanged = glossaryTerms.Keys.ToHash();
		await LocalizationUtil.ClearTranslationsForLocaleGlossaryAsync(locale, _name, glossaryTermsThatChanged, new HashSet<string>(_protectedTerms.Keys));
		_previousFlexibleTerms = _flexibleTerms.Select((KeyValuePair<string, string> p) => new Term(p.Key, p.Value)).ToArray();
		return glossary;
	}

	private void _OnTablePulled(StringTable table)
	{
		if (_glossary != null && (table == null || (string.Equals(_glossary.Result.LanguagePair.TargetLanguageCode, table.LocaleIdentifier.Code, StringComparison.OrdinalIgnoreCase) && string.Equals(table.GlossaryName(), _name, StringComparison.OrdinalIgnoreCase))))
		{
			_ClearGlossary();
		}
	}

	private void _ClearGlossary()
	{
		if (_glossary != null)
		{
			Debug.Log("ClearGlossary: " + _glossary.Result.Name);
			_glossary = null;
			LocalizationUtil.onTablePulled -= _OnTablePulled;
		}
	}

	public void ClearHash()
	{
		_hash = "";
	}

	public async Task<Glossary> GetGlossaryAsync(Locale locale)
	{
		if (_glossary != null)
		{
			return await _glossary;
		}
		LocalizationUtil.onTablePulled += _OnTablePulled;
		return await (_glossary = _CreateGlossaryAsync(locale));
	}

	public async Task<Dictionary<string, string>> GetFlexibleTermsAsync(Locale locale)
	{
		await GetGlossaryAsync(locale);
		return _flexibleTerms;
	}

	public async Task<Dictionary<string, string>> GetProtectedTermsAsync(Locale locale)
	{
		await GetGlossaryAsync(locale);
		return _protectedTerms;
	}

	public async Task<Dictionary<string, string>> GetProperNounsAsync(Locale locale)
	{
		await GetGlossaryAsync(locale);
		return _properNouns;
	}
}
