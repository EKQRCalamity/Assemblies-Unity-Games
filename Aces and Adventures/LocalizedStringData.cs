using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

[ProtoContract]
[UIField]
public class LocalizedStringData : IVariableGroup, IVariable, ITableEntry, ILocalizedString
{
	[ProtoContract]
	[UIField]
	public struct TableEntryId : IVariableGroup, IEquatable<TableEntryId>, ITableEntry
	{
		[ProtoMember(1)]
		private readonly Guid _tableId;

		[ProtoMember(2)]
		private readonly long _entryId;

		public Guid tableId => _tableId;

		public long entryId => _entryId;

		public TableEntryId id => this;

		public TableReference tableReference => _tableId;

		public TableEntryReference entryReference => _entryId;

		public StringTable table
		{
			get
			{
				if (!this)
				{
					return null;
				}
				return tableReference.GetStringTable();
			}
		}

		public StringTableEntry tableEntry
		{
			get
			{
				if (!this)
				{
					return null;
				}
				return tableReference.GetStringTableEntry(entryReference);
			}
		}

		public PoolKeepItemListHandle<StringTableEntry> tableEntries
		{
			get
			{
				PoolKeepItemListHandle<StringTableEntry> poolKeepItemListHandle = Pools.UseKeepItemList<StringTableEntry>();
				if ((bool)this)
				{
					foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
					{
						poolKeepItemListHandle.Add(LocalizationSettings.StringDatabase.GetTableEntry(tableReference, entryReference, locale).Entry ?? LocalizationSettings.StringDatabase.GetTable(tableReference, locale).SetDirtyForEditor().AddEntry(entryId, ""));
					}
					return poolKeepItemListHandle;
				}
				return poolKeepItemListHandle;
			}
		}

		public uint? fileId
		{
			get
			{
				StringTableEntry stringTableEntry = tableEntry;
				if (stringTableEntry != null)
				{
					string[] array = stringTableEntry.Key.Split('/');
					for (int i = 0; i < array.Length; i++)
					{
						if (uint.TryParse(array[i], out var result))
						{
							return result;
						}
					}
				}
				return null;
			}
		}

		public TableEntryId(StringTableEntry stringTableEntry)
		{
			_tableId = stringTableEntry.Table.SharedData.TableCollectionNameGuid;
			_entryId = stringTableEntry.KeyId;
		}

		public TableEntryId(LocalizedString localizedString)
		{
			_tableId = localizedString.TableReference;
			_entryId = localizedString.TableEntryReference;
		}

		public StringTableEntry GetTableEntry(Locale locale)
		{
			if (!this)
			{
				return null;
			}
			return LocalizationSettings.StringDatabase.GetTableEntry(tableReference, entryReference, locale).Entry;
		}

		[Conditional("UNITY_EDITOR")]
		public void RemoveEntryFromTables()
		{
			if (!this)
			{
				return;
			}
			StringTableEntry stringTableEntry = GetTableEntry(LocalizationUtil.ProjectLocale);
			UnityEngine.Debug.Log("Removing Localization Entry From All Tables: " + stringTableEntry.Table.TableCollectionName + "/" + stringTableEntry.Key + " -> " + stringTableEntry.Value);
			SharedTableData sharedData = table.SharedData;
			foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
			{
				LocalizationSettings.StringDatabase.GetTable(tableReference, locale).RemoveEntry(this);
			}
			sharedData.RemoveKey(this);
		}

		public bool TryGetValue(string key, out IVariable value)
		{
			if ((bool)this)
			{
				foreach (IMetadata metadataEntry in tableEntry.MetadataEntries)
				{
					if (metadataEntry is IMetadataVariable metadataVariable && metadataVariable.VariableName == key)
					{
						value = metadataVariable;
						return true;
					}
				}
			}
			value = null;
			return false;
		}

		public override string ToString()
		{
			StringTableEntry stringTableEntry = tableEntry;
			if (stringTableEntry != null)
			{
				try
				{
					return stringTableEntry.GetLocalizedString();
				}
				catch
				{
					return stringTableEntry.Value;
				}
			}
			if (_entryId != 0L)
			{
				return "TABLE ENTRY MISSING";
			}
			return "NULL";
		}

		public bool Equals(TableEntryId other)
		{
			if (_entryId == other._entryId)
			{
				return _tableId.Equals(other._tableId);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TableEntryId other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			long num = _entryId;
			return num.GetHashCode();
		}

		public static bool operator ==(TableEntryId a, TableEntryId b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TableEntryId a, TableEntryId b)
		{
			return !a.Equals(b);
		}

		public static implicit operator Guid(TableEntryId tableEntryId)
		{
			return tableEntryId.tableId;
		}

		public static implicit operator TableReference(TableEntryId tableEntryId)
		{
			return tableEntryId.tableId;
		}

		public static implicit operator long(TableEntryId tableEntryId)
		{
			return tableEntryId.entryId;
		}

		public static implicit operator TableEntryReference(TableEntryId tableEntryId)
		{
			return tableEntryId.entryId;
		}

		public static implicit operator TableEntryId(StringTableEntry stringTableEntry)
		{
			return new TableEntryId(stringTableEntry);
		}

		public static implicit operator TableEntryId(LocalizedString localizedString)
		{
			return new TableEntryId(localizedString);
		}

		public static implicit operator bool(TableEntryId tableEntryId)
		{
			if (tableEntryId.entryId != 0L)
			{
				StringTable stringTable = tableEntryId.tableReference.GetStringTable();
				if ((object)stringTable != null)
				{
					return stringTable.ContainsKey(tableEntryId);
				}
			}
			return false;
		}
	}

	[ProtoContract]
	[UIField]
	[UICategorySort(CategorySortType.Appearance)]
	[ProtoInclude(4, typeof(CharacterNameVariable))]
	[ProtoInclude(5, typeof(IntVariable))]
	[ProtoInclude(6, typeof(LocalizedStringVariable))]
	[ProtoInclude(7, typeof(NestedVariableGroup))]
	[ProtoInclude(8, typeof(BoolVariable))]
	[ProtoInclude(9, typeof(PlayingCardValueVariable))]
	[ProtoInclude(10, typeof(PokerHandTypeVariable))]
	[ProtoInclude(11, typeof(AbilityDescriptionVariable))]
	[ProtoInclude(12, typeof(CombatResultVariable))]
	[ProtoInclude(13, typeof(AbilityNameVariable))]
	[ProtoInclude(14, typeof(EnemyNameVariable))]
	[ProtoInclude(15, typeof(AdventureNameVariable))]
	public abstract class AVariable : IVariable
	{
		[ProtoContract]
		[UIField("Integer", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Primitive")]
		public class IntVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(min = -10, max = 20)]
			[DefaultValue(1)]
			private int _value = 1;

			private IntVariable()
			{
			}

			public IntVariable(int value)
			{
				_value = value;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return _value;
			}
		}

		[ProtoContract]
		[UIField("Boolean", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Primitive")]
		public class BoolVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField]
			[DefaultValue(true)]
			private bool _value = true;

			private BoolVariable()
			{
			}

			public BoolVariable(bool value)
			{
				_value = value;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return _value;
			}
		}

		[ProtoContract]
		[UIField("Table Entry", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Localization")]
		public class LocalizedStringVariable : AVariable, IVariableGroup, ILocalizedString
		{
			[ProtoMember(1)]
			[UIField(validateOnChange = true)]
			private TableEntryId _tableEntryId;

			[ProtoMember(2)]
			[UIField(collapse = UICollapseType.Hide, onValueChangedMethod = "_OnVariablesChange")]
			[UIDeepValueChange]
			private VariableGroup _variables;

			private LocalizedString _localizedString;

			public TableEntryId id => _tableEntryId;

			public LocalizedString localizedString => _CachedLocalizedString(ref _localizedString, _tableEntryId, _variables);

			public VariableGroup variables => _variables ?? (_variables = new VariableGroup());

			public override IEnumerable<TableEntryId> referencedTableEntryIds
			{
				get
				{
					yield return _tableEntryId;
				}
			}

			public override object argument => this;

			private bool _tableEntryIdSpecified => _tableEntryId;

			private bool _variablesSpecified => _variables;

			private bool _hideCheckFormatErrors => _HideCheckFormat(_tableEntryId);

			private bool _hideShowAllLocalizations => !_tableEntryId;

			private LocalizedStringVariable()
			{
			}

			public LocalizedStringVariable(TableEntryId tableEntryId, VariableGroup variableGroup = null)
			{
				_tableEntryId = tableEntryId;
				_variables = variableGroup;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return localizedString.GetSourceValue(selector);
			}

			public bool TryGetValue(string key, out IVariable value)
			{
				return localizedString.TryGetValue(key, out value);
			}

			public override string ToString()
			{
				return localizedString.Localize();
			}

			public static implicit operator bool(LocalizedStringVariable variable)
			{
				return (bool?)variable?._tableEntryId == true;
			}

			private void _OnVariablesChange()
			{
				_localizedString = null;
			}

			private void OnValidateUI()
			{
				_localizedString = null;
			}

			[UIField]
			[UIHideIf("_hideShowAllLocalizations")]
			[UIHorizontalLayout("Buttons")]
			private void _ShowAllLocalizations()
			{
				_ShowAllLocalizationsPopup(this);
			}

			[UIField]
			[UIHideIf("_hideCheckFormatErrors")]
			[UIHorizontalLayout("Buttons")]
			private void _CheckFormatErrors()
			{
				_CheckFormatPopup(localizedString);
			}

			[UIField(validateOnChange = true)]
			[UIHideIf("_hideCheckFormatErrors")]
			[UIHorizontalLayout("Buttons")]
			private void _AutoFillVariables()
			{
				_AutoFillVariablesFromEntry(_tableEntryId, variables);
			}
		}

		[ProtoContract]
		[UIField("Variable Group", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Localization")]
		public class NestedVariableGroup : AVariable, IVariableGroup
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Hide)]
			private VariableGroup _variableGroup;

			public VariableGroup group => _variableGroup ?? (_variableGroup = new VariableGroup());

			public override IEnumerable<TableEntryId> referencedTableEntryIds => _variableGroup?.variables.Values.SelectMany((AVariable v) => v.referencedTableEntryIds) ?? Enumerable.Empty<TableEntryId>();

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return _variableGroup;
			}

			public bool TryGetValue(string key, out IVariable value)
			{
				return _variableGroup.TryGetValue(key, out value);
			}

			public override string ToString()
			{
				if (!_variableGroup)
				{
					return "";
				}
				return $"[{_variableGroup}]";
			}
		}

		[ProtoContract]
		[UIField("Playing Card Value", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Enum")]
		public class PlayingCardValueVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField]
			private PlayingCardValue _value;

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return (int)_value;
			}

			public override string ToString()
			{
				return EnumUtil.Name(_value);
			}
		}

		[ProtoContract]
		[UIField("Poker Hand Type", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Enum")]
		public class PokerHandTypeVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField]
			private PokerHandType _value;

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return (int)_value;
			}

			public override string ToString()
			{
				return EnumUtil.Name(_value);
			}
		}

		[ProtoContract]
		[UIField("Ability Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
		public class AbilityDescriptionVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<AbilityData> _ability;

			private bool _abilitySpecified => _ability.ShouldSerialize();

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return this;
			}

			public override string ToString()
			{
				if (!_ability)
				{
					return "NULL";
				}
				return _ability.data.description;
			}
		}

		[ProtoContract]
		[UIField("Ability Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
		public class AbilityNameVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<AbilityData> _ability;

			private bool _abilitySpecified => _ability.ShouldSerialize();

			private AbilityNameVariable()
			{
			}

			public AbilityNameVariable(DataRef<AbilityData> ability)
			{
				_ability = ability;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return this;
			}

			public override string ToString()
			{
				if (!_ability)
				{
					return "NULL";
				}
				return _ability.data.name;
			}
		}

		[ProtoContract]
		[UIField("Combat Result", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Enum")]
		public class CombatResultVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField]
			private AttackResultType _value;

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return (int)_value;
			}

			public override string ToString()
			{
				return EnumUtil.Name(_value);
			}
		}

		[ProtoContract]
		[UIField("Enemy Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
		public class EnemyNameVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<EnemyData> _enemy;

			private bool _enemySpecified => _enemy.ShouldSerialize();

			private EnemyNameVariable()
			{
			}

			public EnemyNameVariable(DataRef<EnemyData> enemy)
			{
				_enemy = enemy;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return this;
			}

			public override string ToString()
			{
				if (!_enemy)
				{
					return "NULL";
				}
				return _enemy.data.name;
			}
		}

		[ProtoContract]
		[UIField("Adventure Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
		public class AdventureNameVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<AdventureData> _adventure;

			private bool _adventureSpecified => _adventure.ShouldSerialize();

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return this;
			}

			public override string ToString()
			{
				if (!_adventure)
				{
					return "NULL";
				}
				return _adventure.data.GetTitle();
			}
		}

		[ProtoContract]
		[UIField("Character Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Specialized")]
		public class CharacterNameVariable : AVariable
		{
			[ProtoMember(1)]
			[UIField(collapse = UICollapseType.Open)]
			private DataRef<CharacterData> _character;

			private bool _characterSpecified => _character.ShouldSerialize();

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return this;
			}

			public override string ToString()
			{
				if (!_character)
				{
					return "NULL";
				}
				return _character.data.name;
			}
		}

		public class ObjectVariable : AVariable
		{
			private object _object;

			public ObjectVariable(object obj)
			{
				_object = obj;
			}

			public override object GetSourceValue(ISelectorInfo selector)
			{
				return _object;
			}
		}

		public virtual object argument => GetSourceValue(null);

		public virtual IEnumerable<TableEntryId> referencedTableEntryIds
		{
			get
			{
				yield break;
			}
		}

		public abstract object GetSourceValue(ISelectorInfo selector);

		public override string ToString()
		{
			return GetSourceValue(null)?.ToString() ?? "";
		}
	}

	[ProtoContract]
	[UIField]
	public class VariableGroup : IVariableGroup
	{
		private static Dictionary<string, LocalizedVariableName> _StringKeyToEnumKeyMap;

		[ProtoMember(1, OverwriteList = true)]
		[UIField(maxCount = 10)]
		[UIFieldCollectionItem]
		[UIFieldKey(flexibleWidth = 1f)]
		[UIFieldValue(flexibleWidth = 4f)]
		[UIDeepValueChange]
		private Dictionary<LocalizedVariableName, AVariable> _variables;

		private static Dictionary<string, LocalizedVariableName> StringKeyToEnumKeyMap => _StringKeyToEnumKeyMap ?? (_StringKeyToEnumKeyMap = EnumUtil<LocalizedVariableName>.Values.ToDictionarySafeFast((LocalizedVariableName v) => v.ToString(), (LocalizedVariableName v) => v));

		public int Count => variables.Count;

		public Dictionary<LocalizedVariableName, AVariable> variables => _variables ?? (_variables = new Dictionary<LocalizedVariableName, AVariable>());

		public AVariable this[LocalizedVariableName variable]
		{
			get
			{
				return _variables?.GetValueOrDefault(variable);
			}
			set
			{
				variables[variable] = value;
			}
		}

		public void SetData(LocalizedString localizedString)
		{
			if (_variables.IsNullOrEmpty())
			{
				return;
			}
			if (localizedString.Arguments == null)
			{
				IList<object> list2 = (localizedString.Arguments = new List<object>());
			}
			foreach (KeyValuePair<LocalizedVariableName, AVariable> variable in variables)
			{
				localizedString[EnumUtil.Name(variable.Key)] = variable.Value;
				localizedString.Arguments.Add(variable.Value.argument);
			}
		}

		public IEnumerable<T> GetVariables<T>() where T : AVariable
		{
			foreach (AVariable value in variables.Values)
			{
				if (value is AVariable.NestedVariableGroup nestedVariableGroup)
				{
					foreach (T variable in nestedVariableGroup.group.GetVariables<T>())
					{
						yield return variable;
					}
				}
				else if (value is T val)
				{
					yield return val;
				}
			}
		}

		public bool TryGetValue(string key, out IVariable value)
		{
			if (_variables != null && StringKeyToEnumKeyMap.TryGetValue(key, out var value2) && _variables.TryGetValue(value2, out var value3))
			{
				value = value3;
				return true;
			}
			value = null;
			return false;
		}

		public override string ToString()
		{
			return _variables?.ToStringSmart((KeyValuePair<LocalizedVariableName, AVariable> pair) => $"({EnumUtil.Name(pair.Key)}, {pair.Value})") ?? "";
		}

		public static implicit operator bool(VariableGroup variableGroup)
		{
			if (variableGroup == null)
			{
				return false;
			}
			return variableGroup._variables?.Count > 0;
		}
	}

	public const string ADD_TO_LOCALIZATION_TABLE = "Add To Localization Table";

	public const string GET_LOCALIZED_KEY_LABEL_METHOD = "_GetLocalizedKeyLabel";

	private static readonly char[] AutoFillSplit = new char[6] { '{', '}', ':', '(', ')', '|' };

	[ProtoMember(1)]
	[UIField(max = 2048, view = "UI/Input Field Multiline", collapse = UICollapseType.Hide, onValueChangedMethod = "_OnValueChanged")]
	private string _text;

	[ProtoMember(2)]
	private TableEntryId _tableEntryId;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide, onValueChangedMethod = "_OnValueChanged")]
	[UIHideIf("_hideVariables")]
	[UIDeepValueChange]
	private VariableGroup _variables;

	private LocalizedString _localizedString;

	public string rawText
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
		}
	}

	public LocalizedString localizedString => _CachedLocalizedString(ref _localizedString, _tableEntryId, _variables);

	public TableEntryId id
	{
		get
		{
			return _tableEntryId;
		}
		private set
		{
			_tableEntryId = value;
		}
	}

	public VariableGroup variables => _variables ?? (_variables = new VariableGroup());

	private bool _hideAddKey
	{
		get
		{
			if (!id)
			{
				return !DataRefControl.ActiveEditKey;
			}
			return true;
		}
	}

	private bool _hideCheckFormatErrors
	{
		get
		{
			if ((bool)id)
			{
				if (!_variables)
				{
					return _HideCheckFormat(id);
				}
				return false;
			}
			return true;
		}
	}

	private bool _hideShowAllLocalizations => !id;

	private bool _hideVariables => !id;

	private bool _hideRemoveKey
	{
		get
		{
			if ((bool)id)
			{
				return id.fileId == DataRefControl.ActiveEditKey.fileId;
			}
			return true;
		}
	}

	private bool _tableEntryIdSpecified => _tableEntryId;

	private bool _variablesSpecified => _variables;

	private static bool _HideCheckFormat(TableEntryId tableEntryId)
	{
		StringTableEntry tableEntry = tableEntryId.tableEntry;
		if (tableEntry == null)
		{
			return true;
		}
		return !tableEntry.IsSmart;
	}

	private static void _CheckFormatPopup(LocalizedString localizedString)
	{
		string localizedStringError = localizedString.GetLocalizedStringError();
		UIUtil.CreatePopup("Check Format Errors", UIUtil.CreateMessageBox((localizedStringError != null) ? localizedStringError : "No format errors found.", TextAlignmentOptions.MidlineLeft, 32, 1280, 300, 32f), null, null, null, null, null, null, true, true, null, null, null, null, null, null);
	}

	private static void _ShowAllLocalizationsPopup(ILocalizedString localizedString)
	{
		UIUtil.CreatePopup("Localization for: " + localizedString.localizedString.GetPath(), UIUtil.CreateMessageBox(localizedString.GetAllLocalizedStrings().AsEnumerable().ToStringSmart((KeyValuePair<Locale, string> pair) => $"<b>{pair.Key}</b>: {pair.Value}", "\n\n"), TextAlignmentOptions.MidlineLeft, 32, 1280, 720, 32f), null, null, null, null, null, null, true, true, null, null, null, null, null, null);
	}

	public static PoolKeepItemHashSetHandle<LocalizedVariableName> GetNamedVariables(string entry)
	{
		PoolKeepItemHashSetHandle<LocalizedVariableName> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<LocalizedVariableName>();
		string[] array = entry.Split(AutoFillSplit, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			LocalizedVariableName? localizedVariableName = EnumUtil<LocalizedVariableName>.TryParse(array[i]);
			if (localizedVariableName.HasValue)
			{
				LocalizedVariableName valueOrDefault = localizedVariableName.GetValueOrDefault();
				poolKeepItemHashSetHandle.Add(valueOrDefault);
			}
		}
		return poolKeepItemHashSetHandle;
	}

	public static PoolKeepItemHashSetHandle<string> GetPossibleVariables(string entry)
	{
		PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		string[] array = entry.Split(AutoFillSplit, StringSplitOptions.RemoveEmptyEntries);
		foreach (string value in array)
		{
			poolKeepItemHashSetHandle.Add(value);
		}
		return poolKeepItemHashSetHandle;
	}

	public static PoolKeepItemHashSetHandle<string> GetVariableNames(string entry)
	{
		PoolKeepItemHashSetHandle<string> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<string>();
		foreach (string item in from s in entry.GetTextsBetween("{", "}").Concat(entry.GetTextsBetween("{", ":"))
			where s.HasVisibleCharacter() && AutoFillSplit.None(s.Contains)
			select s)
		{
			poolKeepItemHashSetHandle.Add(item);
		}
		return poolKeepItemHashSetHandle;
	}

	private static void _AutoFillVariablesFromEntry(TableEntryId tableEntryId, VariableGroup variables)
	{
		UIUtil.CreateTableEntryMaps(out var tableEntryMap, out var _, new HashSet<TableReference> { "Common" });
		string[] array = tableEntryId.tableEntry.Value.Split(AutoFillSplit, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			LocalizedVariableName? localizedVariableName = EnumUtil<LocalizedVariableName>.TryParse(array[i]);
			if (localizedVariableName.HasValue)
			{
				LocalizedVariableName valueOrDefault = localizedVariableName.GetValueOrDefault();
				LocalizedVariableName variable = valueOrDefault;
				if (variables[variable] == null)
				{
					AVariable aVariable2 = (variables[variable] = new AVariable.LocalizedStringVariable(tableEntryMap.GetValueOrDefault(EnumUtil.FriendlyName(valueOrDefault).FuzzyMatchBestResult(tableEntryMap.Keys))));
				}
			}
		}
		if (variables.Count == 0)
		{
			variables[LocalizedVariableName.Var] = new AVariable.IntVariable(1);
		}
	}

	private static void _RemoveUnusedVariablesFromEntry(string entry, Dictionary<LocalizedVariableName, AVariable> variables)
	{
		using PoolKeepItemHashSetHandle<LocalizedVariableName> poolKeepItemHashSetHandle = GetNamedVariables(entry);
		foreach (LocalizedVariableName item in variables.EnumerateKeysSafe())
		{
			if (!poolKeepItemHashSetHandle.Contains(item))
			{
				variables.Remove(item);
			}
		}
	}

	private static LocalizedString _CachedLocalizedString(ref LocalizedString localizedString, TableEntryId tableEntryId, VariableGroup variables)
	{
		if (localizedString == null)
		{
			localizedString = new LocalizedString(tableEntryId, tableEntryId);
			variables?.SetData(localizedString);
		}
		return localizedString;
	}

	public static IVariable CastToIVariable(object obj)
	{
		if (obj is IVariable result)
		{
			return result;
		}
		if (obj is bool value)
		{
			return new AVariable.BoolVariable(value);
		}
		if (obj is int value2)
		{
			return new AVariable.IntVariable(value2);
		}
		return new AVariable.ObjectVariable(obj);
	}

	private LocalizedStringData()
	{
	}

	public LocalizedStringData(string text)
	{
		_text = text;
	}

	[Conditional("UNITY_EDITOR")]
	public void Register(TableReference tableToRegisterTo, string keyName)
	{
		if (!id)
		{
			id = LocalizationSettings.StringDatabase.GetTable(tableToRegisterTo).AddEntry(keyName, _text);
			id.table.SetDirtyForEditor();
			id.table.SharedData.SetDirtyForEditor();
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void UpdateTableEntry()
	{
		if ((bool)id)
		{
			StringTableEntry tableEntry = id.tableEntry;
			tableEntry.SetValue(_text);
			tableEntry.UpdateIsSmart();
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void ClearId()
	{
		_tableEntryId = default(TableEntryId);
	}

	public void ClearRawText()
	{
		_text = null;
	}

	public bool TryGetValue(string key, out IVariable value)
	{
		return localizedString.TryGetValue(key, out value);
	}

	public object GetSourceValue(ISelectorInfo selector)
	{
		return localizedString.GetSourceValue(selector);
	}

	public override string ToString()
	{
		return this;
	}

	public static implicit operator string(LocalizedStringData data)
	{
		if (data == null)
		{
			return "";
		}
		if (!data.id)
		{
			return data.rawText;
		}
		return data.localizedString.Localize() ?? data.rawText;
	}

	public static implicit operator LocalizedString(LocalizedStringData data)
	{
		return data.localizedString;
	}

	public static implicit operator bool(LocalizedStringData data)
	{
		if (data != null)
		{
			return data.id;
		}
		return false;
	}

	private void _OnValueChanged()
	{
		_localizedString = null;
	}

	private void OnValidateUI()
	{
		_localizedString = null;
	}

	private void _AddKey()
	{
	}

	[UIField]
	[UIHideIf("_hideShowAllLocalizations")]
	[UIHorizontalLayout("Buttons")]
	private void _ShowAllLocalizations()
	{
		_ShowAllLocalizationsPopup(this);
	}

	[UIField]
	[UIHideIf("_hideCheckFormatErrors")]
	[UIHorizontalLayout("Buttons")]
	private void _CheckFormatErrors()
	{
		_CheckFormatPopup(localizedString);
	}

	[UIField(validateOnChange = true)]
	[UIHideIf("_hideCheckFormatErrors")]
	[UIHorizontalLayout("Buttons")]
	private void _RemoveUnusedVariables()
	{
		_RemoveUnusedVariablesFromEntry(id.tableEntry.Value, variables.variables);
	}

	[UIField(validateOnChange = true)]
	[UIHideIf("_hideCheckFormatErrors")]
	[UIHorizontalLayout("Buttons")]
	private void _AutoFillVariables()
	{
		_AutoFillVariablesFromEntry(id, variables);
	}

	[UIField(validateOnChange = true)]
	[UIHideIf("_hideRemoveKey")]
	private void _RemoveFromLocalizationTable()
	{
	}
}
