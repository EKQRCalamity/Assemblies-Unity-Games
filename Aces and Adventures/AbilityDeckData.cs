using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine.Localization.Settings;

[ProtoContract]
[UIField]
public class AbilityDeckData : IDataContent
{
	public const int DECK_SIZE = 30;

	public const int DECK_COUNT = 8;

	public const int MAX_NAME_LENGTH = 20;

	[ProtoMember(1)]
	[UIField]
	private string _name;

	[ProtoMember(2)]
	[UIField(validateOnChange = true, collapse = UICollapseType.Open)]
	private DataRef<CharacterData> _character;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(maxCount = 30, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem(excludedValuesMethod = "_ExcludeAbilities")]
	[UIHideIf("_hideAbilities")]
	private List<DataRef<AbilityData>> _abilities;

	[ProtoMember(4)]
	[UIField(view = "UI/Input Field Multiline", max = 512, collapse = UICollapseType.Open)]
	private string _description;

	[ProtoMember(5)]
	[UIField(tooltip = "This deck, and all of its cards will be unlocked for the player by default.")]
	private bool _unlockedByDefault;

	[ProtoMember(6)]
	[UIField(tooltip = "This deck will not show up in the Deck Editor, even when in Unity.")]
	private bool _hidden;

	[ProtoMember(7)]
	[UIField("DEV Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Used for searching decks in Unity Editor.")]
	private string _devName;

	[ProtoMember(15)]
	public string tags { get; set; }

	public string name
	{
		get
		{
			return LocalizationSettings.StringDatabase.GetTable("Message")?.GetEntry(GenerateAbilityDeckNameEntryMeta.GetKey(_name))?.Value ?? _name;
		}
		set
		{
			_name = value;
		}
	}

	public DataRef<CharacterData> character => _character;

	public PlayerClass characterClass => _character.data.characterClass;

	public IEnumerable<DataRef<AbilityData>> abilities
	{
		get
		{
			if (_abilities.IsNullOrEmpty())
			{
				yield break;
			}
			foreach (DataRef<AbilityData> ability in _abilities)
			{
				yield return ability;
			}
		}
	}

	public PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> abilityCounts
	{
		get
		{
			PoolKeepItemDictionaryHandle<DataRef<AbilityData>, int> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DataRef<AbilityData>, int>();
			if (_abilities != null)
			{
				foreach (DataRef<AbilityData> ability in _abilities)
				{
					poolKeepItemDictionaryHandle[ability] = poolKeepItemDictionaryHandle.value.GetValueOrDefault(ability) + 1;
				}
				return poolKeepItemDictionaryHandle;
			}
			return poolKeepItemDictionaryHandle;
		}
	}

	public string description => _description;

	public int count => _abilities.SafeCount(0);

	public bool isValid => count == 30;

	public bool unlockedByDefault
	{
		get
		{
			return _unlockedByDefault;
		}
		set
		{
			_unlockedByDefault = value;
		}
	}

	public bool hidden => _hidden;

	private bool _hideAbilities => !_character;

	private bool _hideAutoFillDeck
	{
		get
		{
			List<DataRef<AbilityData>> list = _abilities;
			if (list == null || list.Count < 30)
			{
				List<DataRef<AbilityData>> list2 = _abilities;
				if (list2 == null)
				{
					return false;
				}
				return list2.Count == 0;
			}
			return true;
		}
	}

	private bool _characterSpecified => _character.ShouldSerialize();

	public static IEnumerable<DataRef<AbilityDeckData>> Search(PlayerClass? classFilter = null, bool mustBeValid = false)
	{
		foreach (DataRef<AbilityDeckData> item in DataRef<AbilityDeckData>.Search())
		{
			if ((!classFilter.HasValue || item.data.characterClass == classFilter.Value) && (!mustBeValid || item.data.isValid) && (!ContentRef.UGC || !item.isResource) && !item.data.hidden)
			{
				yield return item;
			}
		}
	}

	public AbilityDeckData()
	{
	}

	public AbilityDeckData(string name, DataRef<CharacterData> character)
	{
		_name = name;
		_character = character;
		_abilities = new List<DataRef<AbilityData>>();
		_description = "";
	}

	public AbilityDeckData SetData(string nameToSet, IEnumerable<DataRef<AbilityData>> abilitiesToSet, string descriptionToSet)
	{
		_name = nameToSet;
		_abilities = new List<DataRef<AbilityData>>(abilitiesToSet);
		_description = descriptionToSet;
		return this;
	}

	public string GetTitle()
	{
		if (LaunchManager.InGame || !_devName.HasVisibleCharacter())
		{
			return name;
		}
		return _devName;
	}

	public string GetAutomatedDescription()
	{
		return _description ?? "";
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		_abilities?.RemoveAll((DataRef<AbilityData> d) => !d.ShouldSerialize());
	}

	public string GetSaveErrorMessage()
	{
		if (_name.IsNullOrEmpty())
		{
			return "Please give deck a name before saving.";
		}
		if (!_character)
		{
			return "Please select which character will use this deck before saving.";
		}
		if (_abilities.IsNullOrEmpty())
		{
			return "Please select at least 1 ability before saving.";
		}
		return null;
	}

	public void OnLoadValidation()
	{
	}

	private bool _ExcludeAbilities(DataRef<AbilityData> ability)
	{
		if (_character.data.characterClass == ability.data.characterClass && ability.data.category == AbilityData.Category.Ability && !ability.data.type.IsTrait())
		{
			return _abilities?.Count((DataRef<AbilityData> a) => ContentRef.Equal(a, ability)) >= ability.data.rank.Max();
		}
		return true;
	}

	[UIField(validateOnChange = true)]
	[UIHideIf("_hideAutoFillDeck")]
	private void _AutoFillDeck()
	{
		int num = 30 - _abilities.Count;
		int num2 = _abilities.Count;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			_abilities.Add(ProtoUtil.Clone(_abilities[num3]));
			num3 = (num3 + 1) % num2;
		}
	}

	[UIField(validateOnChange = true)]
	private void _ClearAllButFirst()
	{
		List<DataRef<AbilityData>> list = _abilities;
		if (list != null && list.Count > 1)
		{
			for (int num = _abilities.Count - 1; num >= 1; num--)
			{
				_abilities.RemoveAt(num);
			}
		}
	}

	[UIField]
	private void _Shuffle()
	{
		_abilities?.Shuffle(new Random());
		UIGeneratorType.Validate(this);
	}
}
