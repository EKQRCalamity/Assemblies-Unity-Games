using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizationUtil
{
	public static readonly List<string> USERS;

	public const int MAX_WIDTH = 2105;

	public const string GLOSSARY_TABLE_NAME = "Glossary";

	public const string DEFAULT_GLOSSARY_NAME = "Default";

	public const string CONTEXT_START = "<u>";

	public const string CONTEXT_END = "</u>";

	public static readonly LocaleIdentifier ProjectLocaleIdentifier;

	private static Locale _ProjectLocale;

	private static DefaultLocalizationSettings _Settings;

	private static Dictionary<string, StringTable> _SourceTables;

	private static Dictionary<LocalizedStringData.TableEntryId, string> _ParsedTextMap;

	private static readonly HashSet<LocalizedStringData.TableEntryId> _TableEntriesThatCanBeDirectlyReplaced;

	private static readonly HashSet<LocalizedStringData.TableEntryId> _TableEntriesThatDoNotRequireManualTranslation;

	private static readonly HashSet<LocalizedStringData.TableEntryId> _TableEntriesThatRequireManualTranslation;

	private static readonly Dictionary<Locale, Task<Locale>> _InitializeLocaleTasks;

	private static readonly Dictionary<Locale, Task<StringTable>> _TranslateGlossaryTasks;

	private static bool _HasLocalizedAllDataRefs;

	private static readonly char[] VARIABLE_NAME_SPLIT;

	private static readonly char[] _InvalidSmartBracesSplit;

	private static readonly Dictionary<string, string> _HtmlDecodeMap;

	private static readonly string[] DEFAULT_PROPER_NOUNS;

	private static string DEPLOYED_TEMPLATE_ID => BlobData.GetBlob("Deployment Id");

	public static Locale ProjectLocale
	{
		get
		{
			if (!_ProjectLocale)
			{
				_ProjectLocale = LocalizationSettings.ProjectLocale;
			}
			return _ProjectLocale;
		}
	}

	public static DefaultLocalizationSettings Settings
	{
		get
		{
			object obj = _Settings;
			if (obj == null)
			{
				obj = ProjectLocale.Metadata.GetMetadata<DefaultLocalizationSettings>() ?? new DefaultLocalizationSettings();
				_Settings = (DefaultLocalizationSettings)obj;
			}
			return (DefaultLocalizationSettings)obj;
		}
	}

	private static Dictionary<string, StringTable> SourceTables => _SourceTables ?? (_SourceTables = new Dictionary<string, StringTable>());

	private static Dictionary<LocalizedStringData.TableEntryId, string> ParsedTextMap => _ParsedTextMap ?? (_ParsedTextMap = _CreateParsedTextMap());

	public static bool UsingDiacritics => LocalizationSettings.SelectedLocale.GetProcessTranslationMeta().usesDiacritics;

	public static event Action<StringTable> onTablePushed;

	public static event Action<StringTable> onTablePulled;

	static LocalizationUtil()
	{
		USERS = new List<string>();
		ProjectLocaleIdentifier = "en";
		_TableEntriesThatCanBeDirectlyReplaced = new HashSet<LocalizedStringData.TableEntryId>();
		_TableEntriesThatDoNotRequireManualTranslation = new HashSet<LocalizedStringData.TableEntryId>();
		_TableEntriesThatRequireManualTranslation = new HashSet<LocalizedStringData.TableEntryId>();
		_InitializeLocaleTasks = new Dictionary<Locale, Task<Locale>>();
		_TranslateGlossaryTasks = new Dictionary<Locale, Task<StringTable>>();
		VARIABLE_NAME_SPLIT = new char[3] { '{', ':', '}' };
		_InvalidSmartBracesSplit = new char[4] { '.', ':', '{', '}' };
		_HtmlDecodeMap = new Dictionary<string, string>
		{
			{ "&quot;", "\"" },
			{ "&amp;", "&" },
			{ "&apos;", "'" },
			{ "&#39;", "'" },
			{ "&lt;", "<" },
			{ "&gt;", ">" },
			{ "&nbsp;", " " },
			{ "&ndash;", "-" }
		};
		DEFAULT_PROPER_NOUNS = new string[8] { "I", "I've", "I'll", "I'd", "I'm", "UI", "Discord", "Steam" };
	}

	private static Dictionary<LocalizedStringData.TableEntryId, string> _CreateParsedTextMap()
	{
		_TableEntriesThatCanBeDirectlyReplaced.Clear();
		_TableEntriesThatDoNotRequireManualTranslation.Clear();
		_TableEntriesThatRequireManualTranslation.Clear();
		Dictionary<LocalizedStringData.TableEntryId, string> dictionary = new Dictionary<LocalizedStringData.TableEntryId, string>();
		foreach (Type item in from t in ReflectionUtil.GetTypesWhichImplementInterface<IDataContent>()
			where t.IsConcrete() && t.HasAttribute<LocalizeAttribute>()
			select t)
		{
			foreach (ContentRef item2 in ContentRef.SearchData(item))
			{
				foreach (var (node, localizedStringData) in ReflectionUtil.GetNodesFromUI<LocalizedStringData>(item2.GetDataImmediate()))
				{
					if (!localizedStringData.id)
					{
						continue;
					}
					if (localizedStringData.variables.Count > 0 && localizedStringData.localizedString.GetTableEntry().IsSmart)
					{
						dictionary.Add(localizedStringData.id, localizedStringData);
					}
					if (localizedStringData.id.tableEntry.Value == "{}" && localizedStringData.variables.variables.Values.All((LocalizedStringData.AVariable v) => v.referencedTableEntryIds.All((LocalizedStringData.TableEntryId id) => id.tableEntry.CanBeReplaced())))
					{
						_TableEntriesThatCanBeDirectlyReplaced.Add(localizedStringData.id);
						foreach (LocalizedStringData.AVariable.LocalizedStringVariable variable in localizedStringData.variables.GetVariables<LocalizedStringData.AVariable.LocalizedStringVariable>())
						{
							if ((bool)variable.id)
							{
								_TableEntriesThatRequireManualTranslation.Add(variable.id);
							}
						}
					}
					if (localizedStringData.id.tableEntry.Key.Contains("No translation found for '"))
					{
						localizedStringData.id.tableEntry.Key = $"{item2.friendlyName}/{item2.key.fileId}/{node.GetLocalizationKey()}";
					}
				}
			}
		}
		_ScanForRequiresManualTranslationData(dictionary);
		return dictionary;
	}

	private static void _ScanForRequiresManualTranslationData(Dictionary<LocalizedStringData.TableEntryId, string> parsedText)
	{
		Dictionary<string, string> map = new GlossarySourceGlobalVariables().GetTermsAsync(ProjectLocale).ToListAsync().Result.ToDictionary((GlossarySource.Term t) => t.source, (GlossarySource.Term t) => "");
		Dictionary<string, StringTableEntry> dictionary = GetEntriesThatAreUsedAsGlobalVariables().ToDictionary((KeyValuePair<string, StringTableEntry> p) => p.Key, (KeyValuePair<string, StringTableEntry> p) => p.Value);
		foreach (StringTable allStringTable in ProjectLocale.GetAllStringTables())
		{
			foreach (StringTableEntry value in allStringTable.Values)
			{
				if (!value.IsSmart || parsedText.ContainsKey(value) || value.GetContext().HasVisibleCharacter() || value.Value.ReplaceManyWords(map, StringComparison.Ordinal, (char c) => true, (char c) => true).HasVisibleCharacter())
				{
					continue;
				}
				_TableEntriesThatDoNotRequireManualTranslation.Add(value);
				foreach (KeyValuePair<string, StringTableEntry> item in dictionary)
				{
					if (value.Value.Contains(item.Key, StringComparison.Ordinal))
					{
						_TableEntriesThatRequireManualTranslation.Add(item.Value);
					}
				}
			}
		}
		foreach (LocalizedStringData.AVariable.LocalizedStringVariable item2 in ReflectionUtil.GetValuesFromUI<LocalizedStringData.AVariable.LocalizedStringVariable>(MessageData.Instance))
		{
			if ((bool)item2.id)
			{
				_TableEntriesThatRequireManualTranslation.Add(item2.id);
			}
		}
	}

	public static HashSet<Locale> GetLocalesToSync()
	{
		HashSet<Locale> hashSet = LocalizationSettings.AvailableLocales.Locales.Where((Locale locale) => locale != ProjectLocale && !locale.Mute()).ToHash();
		HashSet<Locale> hashSet2 = new HashSet<Locale>(hashSet.Where((Locale locale) => locale.Solo()));
		if (!hashSet2.Any())
		{
			return hashSet;
		}
		return hashSet2;
	}

	public static async Task SyncAllAsync(Transform uiTransform = null, bool pull = true, bool forcePush = false)
	{
		if ((bool)uiTransform)
		{
			UIUtil.BeginProcessJob(uiTransform);
		}
		ClearCachedData();
		List<Locale> locales = new List<Locale>(GetLocalesToSync());
		await Task.WhenAll(locales.Select(InitializeLocaleSpreadsheetAsync));
		IEnumerable<Locale> source = locales.Where((Locale locale) => locale.HasSpreadsheet());
		List<StringTable> allTables = locales.SelectMany((Locale locale) => LocalizationSettings.StringDatabase.GetAllTables(locale).WaitForCompletion()).ToList();
		await Task.WhenAll((pull ? (from table in source.SelectMany((Locale locale) => LocalizationSettings.StringDatabase.GetAllTables(locale).WaitForCompletion())
			where table.HasValidSheet()
			select table) : Enumerable.Empty<StringTable>()).Select(PullAsync));
		if (Settings.pullOnly)
		{
			if ((bool)uiTransform)
			{
				UIUtil.EndProcess();
			}
			return;
		}
		if (!_HasLocalizedAllDataRefs && (_HasLocalizedAllDataRefs = true))
		{
			await ReflectionUtil.LocalizeAllDataRefsAsync();
		}
		foreach (StringTable item in LocalizationSettings.StringDatabase.GetAllTables(ProjectLocale).WaitForCompletion())
		{
			foreach (GenerateEntryMeta metadata2 in item.SharedData.Metadata.GetMetadatas<GenerateEntryMeta>())
			{
				metadata2.GenerateEntries(item);
			}
		}
		using PoolKeepItemHashSetHandle<SharedTableData> sharedTableDataForcePushHash = Pools.UseKeepItemHashSet(from t in allTables
			select t.SharedData into d
			where d.Metadata.GetMetadata<SharedTableGoogleSheetMeta>()?.ForcePush() ?? false
			select d);
		foreach (SharedTableData item2 in sharedTableDataForcePushHash.value)
		{
			_ = item2;
		}
		using PoolKeepItemHashSetHandle<Locale> localeForcePushHash = Pools.UseKeepItemHashSet(locales.Where((Locale l) => l.SheetMeta()?.ForcePush() ?? false));
		foreach (Locale item3 in localeForcePushHash.value)
		{
			_ = item3;
		}
		foreach (StringTable item4 in allTables)
		{
			TableGoogleSheetMeta metadata = item4.GetMetadata<TableGoogleSheetMeta>();
			if (metadata != null && (forcePush || sharedTableDataForcePushHash.Contains(item4.SharedData) || localeForcePushHash.Contains(item4.Locale())))
			{
				metadata.ClearHashes();
			}
		}
		_ = ParsedTextMap;
		await Task.WhenAll(allTables.Select(PushAsync));
		await Task.WhenAll(locales.Select(SortSheets).Concat(locales.Select(ShareWithEditorGroup)));
		if ((bool)uiTransform)
		{
			UIUtil.EndProcess();
		}
	}

	public static async Task GenerateCharacterSetFiles()
	{
	}

	public static async Task<StringTable> PushAsync(StringTable table)
	{
		return table;
	}

	public static async Task<StringTable> PullAsync(StringTable table)
	{
		if (!table.HasSheet() || !table.HasBeenPushed())
		{
			return table;
		}
		string text = await GoogleUtil.Drive.GetPropertyValueAsync(table.SpreadsheetId(), table.SheetId().ToString());
		if (!text.HasVisibleCharacter() || text == table.GetPullHash())
		{
			return table;
		}
		return await _PullAsync(table, text);
	}

	private static async Task<StringTable> _PullAsync(StringTable table, string pullHash)
	{
		List<StringTableColumn> pulledColumns = (from c in table.GetColumns()
			where c.isPulled
			select c).ToList();
		List<List<CellData>> list = await GoogleUtil.Sheets.GetCellDataByColumnAsync(table.SpreadsheetId(), table.SheetId(), pulledColumns.Select((StringTableColumn c) => c.columnIndex));
		int count = list[0].Count;
		for (int i = 1; i < count; i++)
		{
			CellData cellData = list[0][i];
			if (!cellData.HasValue())
			{
				continue;
			}
			long result;
			SharedTableData.SharedTableEntry sharedTableEntry = (long.TryParse(cellData.Note, out result) ? table.SharedData.GetEntry(result) : null);
			if (sharedTableEntry != null)
			{
				StringTableEntry entry = table.GetEntry(sharedTableEntry.Id) ?? table.AddEntry(sharedTableEntry.Id, "");
				for (int j = 1; j < pulledColumns.Count; j++)
				{
					pulledColumns[j].PullCellData(entry, list[j][i]);
				}
			}
		}
		table.GetOrAddMetadata<TableGoogleSheetMeta>().pullHash = pullHash;
		LocalizationUtil.onTablePulled?.Invoke(table);
		UnityEngine.Debug.Log($"Pulled {table} with hash: {pullHash}");
		return table;
	}

	public static StringTable GetSourceTable(string tableCollectionName)
	{
		return SourceTables.GetValueOrDefault(tableCollectionName) ?? (SourceTables[tableCollectionName] = LocalizationSettings.StringDatabase.GetTable(tableCollectionName, ProjectLocale));
	}

	public static StringTable GetStringTable(this TableReference tableReference, Locale locale = null)
	{
		return LocalizationSettings.StringDatabase.GetTableAsync(tableReference, locale).RunSynchronouslyLockedInEditor();
	}

	public static StringTableEntry GetStringTableEntry(this TableReference tableReference, TableEntryReference entryReference, Locale locale = null)
	{
		return tableReference.GetStringTable(locale).GetEntryFromReference(entryReference);
	}

	public static string GetParsedText(StringTableEntry entry)
	{
		return ParsedTextMap.GetValueOrDefault(new LocalizedStringData.TableEntryId(entry));
	}

	public static void ClearCachedData()
	{
		_InitializeLocaleTasks.Clear();
		_TranslateGlossaryTasks.Clear();
		LocalizationUtil.onTablePulled?.Invoke(null);
	}

	[Conditional("DEV")]
	[Conditional("UNITY_EDITOR")]
	public static void KeepSyncedWithGoogleSheets(float pollInterval = 1f)
	{
		Job.Process(_ListenForSheetChanges(pollInterval), Department.UI);
	}

	private static IEnumerator _ListenForSheetChanges(float pollInterval)
	{
		float timeTillNextSync = 0f;
		UnityEngine.Debug.Log("KeepSyncedWithGoogleSheets Start");
		while (true)
		{
			Locale locale = LocalizationSettings.SelectedLocale;
			if (locale == ProjectLocale || !locale.HasSpreadsheet())
			{
				yield return null;
				continue;
			}
			if (timeTillNextSync <= 0f)
			{
				yield return ToBackgroundThread.Create();
				IDictionary<string, string> properties = GoogleUtil.Drive.GetPropertiesAsync(locale.SheetMeta()).Result;
				yield return ToMainThread.Create();
				if (properties != null)
				{
					Dictionary<int, StringTable> tablesBySheetId = locale.SheetMeta().GetTablesBySheetId(locale);
					List<Task<StringTable>> pullTasks = new List<Task<StringTable>>();
					foreach (KeyValuePair<string, string> item in properties)
					{
						if (int.TryParse(item.Key, out var result))
						{
							StringTable valueOrDefault = tablesBySheetId.GetValueOrDefault(result);
							if ((object)valueOrDefault != null && valueOrDefault.GetPullHash() != item.Value)
							{
								pullTasks.AddReturn(_PullAsync(valueOrDefault, item.Value));
							}
						}
					}
					yield return ToBackgroundThread.Create();
					foreach (Task<StringTable> item2 in pullTasks)
					{
						_ = item2.Result;
					}
					yield return ToMainThread.Create();
					if (pullTasks.Count > 0)
					{
						StringEventLocalizer.OnLocalChange(locale);
					}
				}
				timeTillNextSync = pollInterval;
			}
			timeTillNextSync -= Time.unscaledDeltaTime;
			yield return null;
		}
	}

	public static Locale GetLocaleByCode(string localeCode)
	{
		return LocalizationSettings.AvailableLocales.GetLocale(localeCode);
	}

	public static IEnumerable<string> GetVariableNames(string entry)
	{
		string[] array = entry.Split(VARIABLE_NAME_SPLIT, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			if (char.IsUpper(text[0]) && text.IsAlphanumeric())
			{
				yield return text;
			}
		}
	}

	public static IEnumerable<string> GetVariableNames(this StringTable table)
	{
		foreach (StringTableEntry value in table.Values)
		{
			if (!value.Value.HasVisibleCharacter() || !value.IsSmart)
			{
				continue;
			}
			foreach (string variableName in GetVariableNames(value.Value))
			{
				yield return variableName;
			}
		}
	}

	public static string PostProcessTranslatedSmartText(Locale locale, string sourceText, string translatedText)
	{
		translatedText = locale.GetProcessTranslationMeta().ProcessSmartTranslation(sourceText, translatedText);
		return _PostProcessTranslatedTextCommon(locale, sourceText, translatedText);
	}

	public static string PostProcessTranslatedText(Locale locale, string sourceText, string translatedText)
	{
		return _PostProcessTranslatedTextCommon(locale, sourceText, translatedText);
	}

	private static string _PostProcessTranslatedTextCommon(Locale locale, string sourceText, string translatedText)
	{
		return locale.GetProcessTranslationMeta().ProcessTranslation(sourceText, translatedText);
	}

	public static string ProcessSpacingAfterCharacter(string sourceText, string translatedText, char character)
	{
		Queue<bool> queue = new Queue<bool>();
		char c2 = '\0';
		string text = sourceText;
		foreach (char c3 in text)
		{
			if (c2 == character)
			{
				queue.Enqueue(IsWhiteSpace(c3));
			}
			c2 = c3;
		}
		if (queue.Count == 0)
		{
			return translatedText;
		}
		c2 = '\0';
		StringBuilder stringBuilder = new StringBuilder(translatedText.Length);
		text = translatedText;
		char c4;
		for (int i = 0; i < text.Length; c2 = c4, i++)
		{
			c4 = text[i];
			if (c2 == character)
			{
				bool flag = queue.TryDequeue() == true;
				if (IsWhiteSpace(c4))
				{
					if (!flag)
					{
						continue;
					}
				}
				else if (flag)
				{
					stringBuilder.Append(' ');
				}
			}
			stringBuilder.Append(c4);
		}
		return stringBuilder.ToString();
		static bool IsWhiteSpace(char c)
		{
			if (!char.IsWhiteSpace(c) && c != '.' && c != '?' && c != '!' && c != ',' && c != ';')
			{
				return c == ':';
			}
			return true;
		}
	}

	public static string ProcessSpacingBeforeCharacter(string sourceText, string translatedText, char character, char? sourceCharacterOverride = null)
	{
		Queue<bool> queue = new Queue<bool>();
		char c = sourceCharacterOverride ?? character;
		char c2 = '\0';
		foreach (char num in sourceText)
		{
			if (num == c)
			{
				queue.Enqueue(char.IsWhiteSpace(c2));
			}
			c2 = num;
		}
		StringBuilder stringBuilder = new StringBuilder(translatedText.Length);
		c2 = '\0';
		for (int j = 0; j < translatedText.Length; j++)
		{
			char c3 = translatedText[j];
			char? c4 = ((j < translatedText.Length - 1) ? new char?(translatedText[j + 1]) : null);
			if (!char.IsWhiteSpace(c3) || c4 != character || queue.TryDequeue() == true)
			{
				if (c3 == character && !char.IsWhiteSpace(c2) && queue.TryDequeue() == true)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(c3);
			}
			c2 = c3;
		}
		return stringBuilder.ToString();
	}

	public static string RemoveInvalidSmartBraces(string sourceText, string translatedText)
	{
		if (sourceText.Count((char c) => c == '{') == translatedText.Count((char c) => c == '{'))
		{
			return translatedText;
		}
		StringBuilder stringBuilder = new StringBuilder(translatedText.Length);
		bool flag = false;
		for (int i = 0; i < translatedText.Length; i++)
		{
			char c2 = translatedText[i];
			if (c2 == '{' && i < translatedText.Length - 1)
			{
				string text = "{" + translatedText.Substring(i + 1).Split(_InvalidSmartBracesSplit, 2, StringSplitOptions.None)[0];
				if ((translatedText[i + 1] != '}' || !sourceText.Contains("{}", StringComparison.Ordinal)) && !sourceText.Contains(text, StringComparison.Ordinal))
				{
					i += text.Length - 1;
					continue;
				}
				flag = true;
			}
			else if (c2 == '}')
			{
				if (!flag)
				{
					continue;
				}
				flag = false;
			}
			stringBuilder.Append(c2);
		}
		return stringBuilder.ToString();
	}

	public static string DecodeHtmlText(this string translatedText)
	{
		return translatedText.ReplaceManyWords(_HtmlDecodeMap, StringComparison.OrdinalIgnoreCase, (char c) => true, (char c) => true);
	}

	public static string ProcessContextualTranslations(string targetPhrase, string translationWithTargetMarked, string translationWithNoMarking)
	{
		UnityEngine.Debug.Log("ProcessContextualTranslations(" + targetPhrase + ", " + translationWithTargetMarked + ", " + translationWithNoMarking + ")");
		string text = translationWithTargetMarked.Substring("<u>", "</u>") ?? "";
		string text2 = translationWithNoMarking.Substring("&quot;", "&quot;");
		if (text2 != null && text2.HasLetter())
		{
			return text2;
		}
		foreach (var item in translationWithTargetMarked.Split(new string[1] { "<u>" + text + "</u>" }, StringComparison.OrdinalIgnoreCase))
		{
			string text3 = translationWithNoMarking;
			translationWithNoMarking = translationWithNoMarking.ReplaceSplit(item.text.Trim(), "", StringComparison.OrdinalIgnoreCase, item.position);
			if (text3.Length == translationWithNoMarking.Length)
			{
				return text;
			}
		}
		if (!translationWithNoMarking.HasLetter())
		{
			return text;
		}
		return translationWithNoMarking.Trim();
	}

	public static string ValidateSmartTextFunctionParameters(string sourceText, string translatedText, string functionToValidate = "choose(")
	{
		StringBuilder stringBuilder = new StringBuilder(translatedText.Length);
		int num = 0;
		while (true)
		{
			int num2 = translatedText.IndexOf(functionToValidate, num, StringComparison.Ordinal);
			if (num2 < 0)
			{
				break;
			}
			stringBuilder.Append(translatedText.Substring(num, num2 - num));
			num = num2;
			int num3 = translatedText.LastIndexOf("{", num2, StringComparison.Ordinal);
			if (num3 < 0)
			{
				break;
			}
			string value = translatedText.Substring(num3, num2 + functionToValidate.Length - num3);
			int num4 = sourceText.IndexOf(value, StringComparison.Ordinal);
			if (num4 < 0)
			{
				break;
			}
			num4 += num2 - num3;
			int num5 = sourceText.IndexOf(")", num4 + functionToValidate.Length, StringComparison.Ordinal);
			if (num5 < 0)
			{
				break;
			}
			stringBuilder.Append(sourceText.Substring(num4, num5 + 1 - num4));
			num = translatedText.IndexOf(":", num2 + functionToValidate.Length, StringComparison.Ordinal);
			if (num < 0)
			{
				return sourceText;
			}
		}
		if (num < translatedText.Length)
		{
			stringBuilder.Append(translatedText.Substring(num, translatedText.Length - num));
		}
		return stringBuilder.ToString();
	}

	public static void ClearIds(object value)
	{
		foreach (LocalizedStringData item in ReflectionUtil.GetValuesFromUI<LocalizedStringData>(value))
		{
			_ = item;
		}
	}

	public static HashSet<LocalizedStringData.TableEntryId> TestDirectlyReplaced()
	{
		_ = ParsedTextMap;
		return _TableEntriesThatCanBeDirectlyReplaced;
	}

	public static HashSet<LocalizedStringData.TableEntryId> TestDoNotRequireManualTranslation()
	{
		_ = ParsedTextMap;
		return _TableEntriesThatDoNotRequireManualTranslation;
	}

	public static HashSet<LocalizedStringData.TableEntryId> TestRequireManualTranslation()
	{
		_ = ParsedTextMap;
		return _TableEntriesThatRequireManualTranslation;
	}

	public static IEnumerable<StringTable> GetAllTablesWithSheets()
	{
		foreach (Locale item in from locale in GetLocalesToSync()
			where locale.HasSpreadsheet()
			select locale)
		{
			foreach (StringTable item2 in LocalizationSettings.StringDatabase.GetAllTables(item).WaitForCompletion())
			{
				if (item2.SheetMeta() != null)
				{
					yield return item2;
				}
			}
		}
	}

	public static IEnumerable<StringTableEntry> GetAllEntriesThatBelongToSpreadsheet()
	{
		foreach (StringTable allTablesWithSheet in GetAllTablesWithSheets())
		{
			foreach (StringTableEntry value in allTablesWithSheet.Values)
			{
				yield return value;
			}
		}
	}

	public static IEnumerable<StringTableEntry> GetOutOfDateEntries()
	{
		foreach (StringTableEntry item in GetAllEntriesThatBelongToSpreadsheet())
		{
			TranslationModeMeta metadata = item.GetMetadata<TranslationModeMeta>();
			if (metadata != null && metadata.translationIsOutOfDate && metadata.value == TranslationMode.Manual)
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<StringTableEntry> GetEntriesThatDoNotRequireManualTranslation()
	{
		foreach (StringTableEntry item in GetAllEntriesThatBelongToSpreadsheet())
		{
			if (!item.UsesManualTranslation())
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<KeyValuePair<string, StringTableEntry>> GetEntriesThatAreUsedAsGlobalVariables()
	{
		PersistentVariablesSource globalVariableSource = LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions.OfType<PersistentVariablesSource>().FirstOrDefault();
		if (globalVariableSource == null)
		{
			yield break;
		}
		foreach (string globalVariableName in globalVariableSource.Keys)
		{
			foreach (string key in globalVariableSource[globalVariableName].Keys)
			{
				if (globalVariableSource[globalVariableName][key] is LocalizedString localizedString)
				{
					yield return new KeyValuePair<string, StringTableEntry>("{" + globalVariableName + "." + key + "}", localizedString.GetTableEntry());
				}
			}
		}
	}

	public static bool ShouldForcePush()
	{
		if (!Settings.ForcePush())
		{
			return false;
		}
		return true;
	}

	public static async Task CheckForErrors()
	{
		int[] argumentPlaceholders = new int[10];
		HashSet<StringTableEntry> entriesWithErrors = new HashSet<StringTableEntry>();
		foreach (Locale locale in GetLocalesToSync())
		{
			foreach (StringTable sourceTable in ProjectLocale.GetAllStringTables())
			{
				StringTable localeTable = LocalizationSettings.StringDatabase.GetTable(sourceTable.TableCollectionName, locale);
				foreach (StringTableEntry value3 in sourceTable.Values)
				{
					StringTableEntry localeEntry = localeTable.GetEntry(value3.KeyId);
					if (localeEntry == null || !localeEntry.IsManuallyTranslated())
					{
						continue;
					}
					string value = localeEntry.Value;
					if (value == null || !value.HasVisibleCharacter())
					{
						continue;
					}
					string value2 = value3.GetPushedSource().value;
					if (char.IsWhiteSpace(value[0]) && !char.IsWhiteSpace(value2[0]))
					{
						entriesWithErrors.Add(localeEntry);
					}
					else if (char.IsWhiteSpace(value[^1]) && !char.IsWhiteSpace(value2[^1]))
					{
						entriesWithErrors.Add(localeEntry);
					}
					else if (value.HasExcessSpacing())
					{
						entriesWithErrors.Add(localeEntry);
					}
					else
					{
						if (!value3.IsSmart || !localeEntry.IsSmart)
						{
							continue;
						}
						using PoolKeepItemHashSetHandle<string> localeVariables = LocalizedStringData.GetVariableNames(value);
						if (!localeVariables.value.ContainsAll(LocalizedStringData.GetVariableNames(value3.Value).AsEnumerable()))
						{
							entriesWithErrors.Add(localeEntry);
							continue;
						}
						try
						{
							LocalizedString localizedString = new LocalizedString(sourceTable.TableCollectionName, localeEntry.KeyId)
							{
								LocaleOverride = locale
							};
							localizedString.SetArguments(argumentPlaceholders);
							foreach (string item in LocalizedStringData.GetNamedVariables(localeEntry.Value).AsEnumerable().Select(EnumUtil.Name)
								.Concat(LocalizedStringData.GetPossibleVariables(value3.Value).AsEnumerable())
								.Distinct())
							{
								localizedString[item] = new LocalizedStringData.AVariable.IntVariable(0);
							}
							await localizedString.GetLocalizedStringAsync().Task;
						}
						catch
						{
							entriesWithErrors.Add(localeEntry);
						}
					}
				}
			}
		}
		foreach (StringTableEntry item2 in entriesWithErrors)
		{
			UnityEngine.Debug.Log(item2.Table.TableCollectionName + "/" + item2.Key + " -> [" + item2.Value + "]");
		}
	}

	private static LocaleGoogleSheetMeta SheetMeta(this Locale locale)
	{
		return locale.Metadata.GetMetadata<LocaleGoogleSheetMeta>();
	}

	private static async Task<bool> HasValidSpreadsheetAsync(this Locale locale)
	{
		LocaleGoogleSheetMeta localeGoogleSheetMeta = locale.SheetMeta();
		bool flag = localeGoogleSheetMeta != null && (bool)localeGoogleSheetMeta;
		if (flag)
		{
			flag = await GoogleUtil.Sheets.SpreadsheetExistsAsync(localeGoogleSheetMeta);
		}
		return flag;
	}

	private static bool HasSpreadsheet(this Locale locale)
	{
		return locale.SheetMeta();
	}

	private static bool UsesGenderColumn(this Locale locale)
	{
		return locale.SheetMeta()?.usesGenderColumn ?? false;
	}

	public static IList<StringTable> GetAllStringTables(this Locale locale)
	{
		return LocalizationSettings.StringDatabase.GetAllTables(locale).WaitForCompletion();
	}

	public static async Task<IList<Sheet>> CacheAllSheets(this Locale locale)
	{
		LocaleGoogleSheetMeta spreadsheetMeta = locale.SheetMeta();
		if (!spreadsheetMeta)
		{
			return null;
		}
		IList<Sheet> list = await GoogleUtil.Sheets.GetAllSheets(spreadsheetMeta);
		Dictionary<int, StringTable> tablesBySheetId = spreadsheetMeta.GetTablesBySheetId(locale);
		foreach (Sheet item in list)
		{
			int? sheetId = item.Properties.SheetId;
			if (sheetId.HasValue)
			{
				int valueOrDefault = sheetId.GetValueOrDefault();
				if (tablesBySheetId.ContainsKey(valueOrDefault))
				{
					tablesBySheetId[valueOrDefault].GetOrAddMetadata<TableGoogleSheetMeta>().sheet = item;
				}
			}
		}
		return list;
	}

	public static ProcessTranslationMeta GetProcessTranslationMeta(this Locale locale)
	{
		return locale.Metadata.GetMetadata<ProcessTranslationMeta>() ?? ProcessTranslationMeta.Default;
	}

	public static bool IsWorkInProgress(this Locale locale)
	{
		FinishedMeta metadata = locale.Metadata.GetMetadata<FinishedMeta>();
		if (metadata == null)
		{
			return true;
		}
		return !metadata.isFinished;
	}

	public static bool ExcludeFromSelection(this Locale locale)
	{
		return locale.Metadata.GetMetadata<FinishedMeta>()?.excludeFromSelection ?? false;
	}

	public static float SortPriority(this Locale locale)
	{
		return locale.Metadata.GetMetadata<SortMeta>()?.priority ?? (-1f);
	}

	public static bool Solo(this Locale locale)
	{
		return locale.Metadata.HasMetadata<SoloMeta>();
	}

	public static void SetSolo(this Locale locale, bool solo)
	{
		locale.Metadata.AddOrRemove<SoloMeta>(solo);
	}

	public static bool Mute(this Locale locale)
	{
		return locale.Metadata.HasMetadata<MuteMeta>();
	}

	public static void AddOrRemove<T>(this MetadataCollection metadata, bool add) where T : IMetadata, new()
	{
		if (add)
		{
			metadata.GetOrAddMetadata<T>();
			return;
		}
		T metadata2 = metadata.GetMetadata<T>();
		if (metadata2 != null)
		{
			metadata.RemoveMetadata(metadata2);
		}
	}

	private static TableGoogleSheetMeta SheetMeta(this StringTable table)
	{
		return table.GetMetadata<TableGoogleSheetMeta>();
	}

	private static string SpreadsheetId(this StringTable table)
	{
		return LocalizationSettings.Instance.GetAvailableLocales().GetLocale(table.LocaleIdentifier).SheetMeta();
	}

	public static int SheetId(this StringTable table)
	{
		return table.SheetMeta();
	}

	private static string GetPushHash(this StringTable table)
	{
		return table.SheetMeta()?.pushHash;
	}

	private static bool HasBeenPushed(this StringTable table)
	{
		return table.GetPushHash().HasVisibleCharacter();
	}

	private static string GetHash128(this StringTable table)
	{
		TableGoogleSheetMeta tableGoogleSheetMeta = table.SheetMeta();
		if (tableGoogleSheetMeta == null)
		{
			return GameUtil.GetHash128(table);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (StringTableColumn column in tableGoogleSheetMeta.columns)
		{
			foreach (SimpleCellData pushedSimpleCellDatum in column.GetPushedSimpleCellData(table))
			{
				stringBuilder.Append(pushedSimpleCellDatum.ToString());
			}
		}
		return stringBuilder.ToString().ToHash128();
	}

	private static string GetPullHash(this StringTable table)
	{
		return table.SheetMeta()?.pullHash;
	}

	private static bool HasValidSheet(this StringTable table)
	{
		return table.SheetMeta()?.sheet != null;
	}

	private static bool HasSheet(this StringTable table)
	{
		if ((bool)table.SheetMeta())
		{
			return table.Locale().SheetMeta();
		}
		return false;
	}

	public static List<StringTableColumn> GetColumns(this StringTable table)
	{
		return table.GetOrAddMetadata<TableGoogleSheetMeta>().columns;
	}

	public static bool AddColumnToSheet<T>(this StringTable table) where T : StringTableColumn, new()
	{
		List<StringTableColumn> columns = table.GetColumns();
		if (columns != null && !columns.Contains((StringTableColumn t) => t is T))
		{
			T val = new T();
			columns.Add(val);
			val.columnIndex = columns.Count - 1;
			return true;
		}
		return false;
	}

	private static Locale Locale(this LocalizationTable table)
	{
		return LocalizationSettings.AvailableLocales.GetLocale(table.LocaleIdentifier);
	}

	public static string GlossaryName(this StringTable table)
	{
		return table.GetMetadata<GlossaryNameMeta>()?.name ?? table.SharedData.Metadata.GetMetadata<GlossaryNameMeta>()?.name ?? "Default";
	}

	private static GlossaryData GetGlossaryData(this StringTable table, string glossaryNameOverride = null)
	{
		if ((object)table == null)
		{
			return null;
		}
		Locale locale = table.Locale();
		if ((object)locale == null)
		{
			return null;
		}
		return locale.SheetMeta()?.GetGlossaryData(glossaryNameOverride ?? table.GlossaryName());
	}

	public static IEnumerable<StringTableEntry> EntriesOrderedByKey(this StringTable table)
	{
		return table.Values.OrderBy((StringTableEntry e) => e.Key);
	}

	public static GoogleUtil.Sheets.WrapStrategy? GetWrapMode(this StringTable table)
	{
		SharedTableGoogleSheetMeta metadata = table.SharedData.Metadata.GetMetadata<SharedTableGoogleSheetMeta>();
		if (metadata == null || metadata.textWrapMode == GoogleUtil.Sheets.WrapStrategy.DEFAULT)
		{
			return null;
		}
		return metadata.textWrapMode;
	}

	public static void MarkDirtyForPush(this StringTable table)
	{
		TableGoogleSheetMeta tableGoogleSheetMeta = (((object)table != null) ? table.SheetMeta() : null);
		if (tableGoogleSheetMeta != null)
		{
			tableGoogleSheetMeta.pushHash = "";
		}
	}

	public static string GetComment(this StringTableEntry entry, bool fallbackToSharedEntry = false)
	{
		object obj = entry.GetMetadata<Comment>()?.CommentText;
		if (obj == null)
		{
			if (!fallbackToSharedEntry)
			{
				return null;
			}
			Comment metadata = entry.SharedEntry.Metadata.GetMetadata<Comment>();
			if (metadata == null)
			{
				return null;
			}
			obj = metadata.CommentText;
		}
		return (string)obj;
	}

	public static StringTableEntry GetSourceEntry(this StringTableEntry entry)
	{
		return GetSourceTable(entry.Table.TableCollectionName)[entry.KeyId];
	}

	public static ContextMeta GetContextMeta(this StringTableEntry entry)
	{
		return entry.GetMetadata<ContextMeta>() ?? entry.GetSourceEntry().GetMetadata<ContextMeta>() ?? entry.SharedEntry.Metadata.GetMetadata<ContextMeta>();
	}

	public static string GetContext(this StringTableEntry entry)
	{
		return entry.GetContextMeta()?.context;
	}

	public static string GetContextComment(this StringTableEntry entry)
	{
		ContextMeta contextMeta = entry.GetContextMeta();
		if (contextMeta == null)
		{
			return null;
		}
		if (!contextMeta.aliasContext.HasVisibleCharacter())
		{
			return contextMeta.context;
		}
		return "[" + contextMeta.context + "]\n" + contextMeta.aliasContext;
	}

	public static string GlossaryName(this StringTableEntry entry)
	{
		return entry.GetMetadata<GlossaryNameMeta>()?.name ?? ((StringTable)entry.Table).GlossaryName();
	}

	public static GlossaryEntryMeta GlossaryMeta(this StringTableEntry entry)
	{
		return entry.GetMetadata<GlossaryEntryMeta>() ?? entry.SharedEntry.Metadata.GetMetadata<GlossaryEntryMeta>();
	}

	public static IncludeAlternateCasing IncludeAlternatingCasingsInGlossary(this StringTableEntry entry)
	{
		return entry.GlossaryMeta()?.includeAlternateCasing ?? IncludeAlternateCasing.All;
	}

	public static bool IsExcludedFromDefaultGlossary(this StringTableEntry entry)
	{
		GlossaryEntryMeta glossaryEntryMeta = entry.GlossaryMeta();
		if (glossaryEntryMeta != null)
		{
			switch (glossaryEntryMeta.excludeFromDefault)
			{
			default:
				return EnumUtil.FlagCount(glossaryEntryMeta.tags) != 0;
			case ExcludeFromDefaultGlossary.Always:
				return true;
			case ExcludeFromDefaultGlossary.Never:
				break;
			}
		}
		return false;
	}

	public static bool HasGlossaryTags(this StringTableEntry entry, GlossaryTags tags)
	{
		GlossaryEntryMeta glossaryEntryMeta = entry.GlossaryMeta();
		if (glossaryEntryMeta == null)
		{
			return false;
		}
		return (glossaryEntryMeta.tags & tags) == tags;
	}

	public static Dictionary<string, string> GetProcessedTerms(this StringTableEntry entry, Dictionary<string, string> terms, Locale locale)
	{
		AddedTermsMeta addedTermsMeta = entry.GetMetadata<AddedTermsMeta>() ?? entry.SharedEntry.Metadata.GetMetadata<AddedTermsMeta>();
		ExcludedTermsMeta excludedTermsMeta = entry.GetMetadata<ExcludedTermsMeta>() ?? entry.SharedEntry.Metadata.GetMetadata<ExcludedTermsMeta>();
		bool flag = addedTermsMeta != null && addedTermsMeta.terms?.Count > 0;
		bool flag2 = excludedTermsMeta != null && excludedTermsMeta.terms?.Count > 0;
		if (!flag && !flag2)
		{
			return terms;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(terms);
		if (flag)
		{
			foreach (LocalizedString term in addedTermsMeta.terms)
			{
				foreach (GlossarySource.Term item in GlossarySourceStringTable.GetTermsForEntry(entry, term.Value(ProjectLocale), term.Value(locale)))
				{
					dictionary[item.source] = item.target;
				}
			}
		}
		if (flag2)
		{
			foreach (LocalizedString term2 in excludedTermsMeta.terms)
			{
				foreach (GlossarySource.Term item2 in GlossarySourceStringTable.GetTermsForEntry(entry, term2.Value(ProjectLocale), term2.Value(locale)))
				{
					dictionary.Remove(item2.source);
				}
			}
			return dictionary;
		}
		return dictionary;
	}

	public static IList<string> GetProcessedProtectedTerms(this StringTableEntry entry, IList<string> protectedTerms, Locale locale)
	{
		AddedTermsMeta addedTermsMeta = entry.GetMetadata<AddedTermsMeta>() ?? entry.SharedEntry.Metadata.GetMetadata<AddedTermsMeta>();
		if (addedTermsMeta == null || !(addedTermsMeta.terms?.Count > 0))
		{
			return protectedTerms;
		}
		List<string> list = new List<string>(protectedTerms);
		foreach (LocalizedString term in addedTermsMeta.terms)
		{
			foreach (GlossarySource.Term item in GlossarySourceStringTable.GetTermsForEntry(entry, term.Value(ProjectLocale), term.Value(locale)))
			{
				list.Remove(item.source);
			}
		}
		return list;
	}

	public static bool ShouldReverseTranslate(this StringTableEntry entry)
	{
		return entry.GetMetadata<TranslationModeMeta>()?.lastUsedMode.ShouldReverseTranslate() ?? false;
	}

	public static string GetTextToCheckAgainstReverseTranslation(this StringTableEntry entry)
	{
		StringTableEntry sourceEntry = entry.GetSourceEntry();
		TranslationModeMeta metadata = entry.GetMetadata<TranslationModeMeta>();
		if (metadata == null)
		{
			return sourceEntry.Value;
		}
		return metadata.lastUsedMode switch
		{
			TranslationMode.Parsed => GetParsedText(sourceEntry), 
			TranslationMode.Alias => entry.GetContext(), 
			TranslationMode.AliasContext => entry.GetContext(), 
			_ => sourceEntry.Value, 
		};
	}

	public static string TryGetParsedText(this StringTableEntry entry)
	{
		return GetParsedText(entry) ?? entry.Value;
	}

	public static bool IsReverseTranslated(this StringTableEntry entry)
	{
		TranslationModeMeta metadata = entry.GetSourceEntry().GetMetadata<TranslationModeMeta>();
		return ((metadata != null) ? new TranslationMode?(metadata.value) : entry.GetMetadata<TranslationModeMeta>()?.value) == TranslationMode.ReverseTranslate;
	}

	public static bool IsReplaceTranslated(this StringTableEntry entry)
	{
		TranslationModeMeta metadata = entry.GetSourceEntry().GetMetadata<TranslationModeMeta>();
		if (((metadata != null) ? new TranslationMode?(metadata.value) : entry.GetMetadata<TranslationModeMeta>()?.lastUsedMode) != TranslationMode.Replace && !_TableEntriesThatCanBeDirectlyReplaced.Contains(entry))
		{
			return _TableEntriesThatDoNotRequireManualTranslation.Contains(entry);
		}
		return true;
	}

	public static bool CanBeReplaced(this StringTableEntry entry)
	{
		return entry?.SharedEntry.Metadata.GetMetadata<CanBeReplacedMeta>() != null;
	}

	private static bool? _UsesManualTranslation(this StringTableEntry entry)
	{
		if (!_TableEntriesThatRequireManualTranslation.Contains(entry))
		{
			return null;
		}
		return true;
	}

	public static bool UsesManualTranslation(this StringTableEntry entry)
	{
		return entry.SharedEntry.Metadata.GetMetadata<UsesManualTranslationMeta>()?.usesManualTranslation ?? entry._UsesManualTranslation() ?? entry.Table.SharedData.Metadata.GetMetadata<UsesManualTranslationMeta>()?.usesManualTranslation ?? (!entry.IsReplaceTranslated());
	}

	public static bool AutomatedTranslationChecked(this StringTableEntry entry)
	{
		ItemTranslationChecked metadata = entry.GetMetadata<ItemTranslationChecked>();
		if (metadata == null)
		{
			return false;
		}
		return metadata.value == ItemTranslationChecked.Checked.Y;
	}

	public static bool IsManuallyTranslated(this StringTableEntry entry)
	{
		TranslationModeMeta metadata = entry.GetMetadata<TranslationModeMeta>();
		if (metadata == null)
		{
			return false;
		}
		return metadata.value == TranslationMode.Manual;
	}

	public static void ClearAutomatedTranslationCheck(this StringTableEntry entry)
	{
		ItemTranslationChecked metadata = entry.GetMetadata<ItemTranslationChecked>();
		if (metadata != null)
		{
			entry.RemoveMetadata(metadata);
		}
	}

	public static string GetLastCheckedAutomatedTranslation(this StringTableEntry entry)
	{
		ItemTranslationChecked metadata = entry.GetMetadata<ItemTranslationChecked>();
		if (metadata == null || metadata.value != ItemTranslationChecked.Checked.Y)
		{
			return null;
		}
		return metadata.lastAutomatedTranslation ?? "";
	}

	public static void SetLastPulledSource(this StringTableEntry pulledEntry, SimpleCellData pulledData)
	{
		pulledEntry.GetOrAddMetadata<TranslationModeMeta>().lastPulledSource = pulledData.value;
	}

	public static string GetLastPulledSource(this StringTableEntry entry)
	{
		string text = entry.GetMetadata<TranslationModeMeta>()?.lastPulledSource;
		if (text == null || !text.HasVisibleCharacter())
		{
			return null;
		}
		return text;
	}

	public static SimpleCellData GetPushedSource(this StringTableEntry sourceEntry)
	{
		string parsedText = GetParsedText(sourceEntry);
		return new SimpleCellData(parsedText ?? sourceEntry.Value, ((parsedText != null) ? sourceEntry.Value : null) ?? sourceEntry.GetComment());
	}

	public static StringTable StringTable(this StringTableEntry sourceEntry)
	{
		return sourceEntry?.Table as StringTable;
	}

	private static bool IsSmart(this string s)
	{
		return s?.Contains('{') ?? false;
	}

	public static bool IsContextual(this TranslationMode mode)
	{
		if (mode != TranslationMode.Context)
		{
			return mode == TranslationMode.AliasContext;
		}
		return true;
	}

	public static bool ShouldReverseTranslate(this TranslationMode mode)
	{
		return mode switch
		{
			TranslationMode.Parsed => true, 
			TranslationMode.Simple => true, 
			TranslationMode.Context => true, 
			TranslationMode.Alias => true, 
			TranslationMode.AliasContext => true, 
			_ => false, 
		};
	}

	public static T OptimizeReference<T>(this T localizedReference) where T : LocalizedReference
	{
		return localizedReference;
	}

	private static T RunSynchronously<T>(this AsyncOperationHandle<T> asyncOperation)
	{
		return asyncOperation.Task.Result;
	}

	private static T RunSynchronouslyLocked<T>(this AsyncOperationHandle<T> asyncOperation)
	{
		lock (asyncOperation.Task)
		{
			return asyncOperation.Task.Result;
		}
	}

	private static T RunSynchronouslyLockedInEditor<T>(this AsyncOperationHandle<T> asyncOperation)
	{
		return asyncOperation.Task.Result;
	}

	public static async Task<Locale> InitializeLocaleSpreadsheetAsync(Locale locale)
	{
		return await (_InitializeLocaleTasks.GetValueOrDefault(locale) ?? (_InitializeLocaleTasks[locale] = _InitializeLocaleSpreadsheetAsync(locale)));
	}

	private static async Task<Locale> _InitializeLocaleSpreadsheetAsync(Locale locale)
	{
		return locale;
	}

	public static async Task<StringTable> InitializeTableSheetAsync(StringTable table)
	{
		return table;
	}

	private static async Task _DeleteDefaultSheet(Locale locale)
	{
		LocaleGoogleSheetMeta sheetMeta = locale.SheetMeta();
		if ((bool)sheetMeta && !sheetMeta.hasDeletedDefaultSheet)
		{
			bool defaultSheetExists = await GoogleUtil.Sheets.SheetExistsAsync(sheetMeta, 0);
			bool flag = false;
			if (defaultSheetExists)
			{
				flag = (await GoogleUtil.Sheets.DeleteSheetAsync(sheetMeta, 0))?.Any() ?? false;
			}
			sheetMeta.hasDeletedDefaultSheet = !defaultSheetExists || flag;
		}
	}

	public static async Task SortSheets(Locale locale)
	{
		await _DeleteDefaultSheet(locale);
		List<Request> list = new List<Request>();
		foreach (StringTable item in from t in LocalizationSettings.StringDatabase.GetAllTables(locale).WaitForCompletion()
			where t.SheetMeta()
			orderby t.TableCollectionName
			select t)
		{
			list.Add(GoogleUtil.Sheets.MoveSheetRequest(item.SheetId(), list.Count));
		}
		await GoogleUtil.Sheets.BatchUpdateAsync(locale.SheetMeta(), list);
	}

	public static async Task ShareWithEditorGroup(Locale locale)
	{
		LocaleGoogleSheetMeta meta = locale.Metadata.GetMetadata<LocaleGoogleSheetMeta>();
		if (!meta)
		{
			return;
		}
		string name = $"Aces and Adventures {locale.Identifier}";
		string linkName = GoogleUtil.Mail.ToEmailName(name).RemoveFromEnd('-');
		string groupEmail = linkName + "@triplebtitles.com";
		string description = $"Grants edit access to {locale.Identifier} localization spreadsheet.";
		if ((await GoogleUtil.AdminDirectory.GetOrCreateGroup(groupEmail, name, description)).Out(out var createdGroup))
		{
			UnityEngine.Debug.Log("Created Google Group: " + createdGroup.Email);
			await GoogleUtil.TryRepeat(() => GoogleUtil.GroupSettings.CopyTo(createdGroup.Email), 3f, 20, waitBeforeFirstAttempt: true);
			await GoogleUtil.Drive.ShareWithEmail(meta, createdGroup.Email, GoogleUtil.Drive.PermissionRole.writer, GoogleUtil.Drive.PermissionType.group);
			await GoogleUtil.AdminDirectory.UpdateOrAddMembersToGroup(createdGroup.Email, USERS.Where((string user) => user != GoogleUtil.SERVICE_ACCOUNT_EMAIL), GoogleUtil.AdminDirectory.MemberRole.OWNER);
			await _SendLinkEmailToGroup(locale, createdGroup.Email, meta, linkName);
		}
		else if (meta != meta.linkedSpreadsheetId)
		{
			await GoogleUtil.Drive.ShareWithEmail(meta, groupEmail, GoogleUtil.Drive.PermissionRole.writer, GoogleUtil.Drive.PermissionType.group);
			await GoogleUtil.Rebrandly.Links.UpdateOrCreate(await GoogleUtil.Drive.GetLink(meta), linkName);
			meta.linkedSpreadsheetId = meta;
		}
	}

	private static async Task _SendLinkEmailToGroup(Locale locale, string groupEmail, LocaleGoogleSheetMeta meta, string linkName)
	{
		GoogleUtil.Mail.User user = GoogleUtil.Mail.AsUser(GoogleUtil.ORG_EMAIL);
		string subject = $"Link to {locale.Identifier} localization spreadsheet.";
		await user.Send(groupEmail, subject, (await GoogleUtil.Rebrandly.Links.UpdateOrCreate(await GoogleUtil.Drive.GetLink(meta), linkName)).ShortURL);
		meta.linkedSpreadsheetId = meta;
	}

	public static async Task<StringTable> TranslateTableAsync(StringTable table, bool useGlossary = true, Func<LocalizedStringData.TableEntryId, bool> shouldTranslateSourceEntry = null, string glossaryOverride = null)
	{
		if (table.TableCollectionName == "Glossary")
		{
			return await (_TranslateGlossaryTasks.GetValueOrDefault(table.Locale()) ?? (_TranslateGlossaryTasks[table.Locale()] = _TranslateTableAsync(table, useGlossary, shouldTranslateSourceEntry, glossaryOverride)));
		}
		return await _TranslateTableAsync(table, useGlossary, shouldTranslateSourceEntry, glossaryOverride);
	}

	private static async Task<StringTable> _TranslateTableAsync(StringTable table, bool useGlossary = true, Func<LocalizedStringData.TableEntryId, bool> shouldTranslateSourceEntry = null, string glossaryOverride = null)
	{
		return table;
	}

	public static async Task ClearTranslationsForLocaleGlossaryAsync(Locale locale, string glossaryName, HashSet<string> glossaryTermsThatChanged, HashSet<string> protectedTerms)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void CreateMissingTables()
	{
	}
}
