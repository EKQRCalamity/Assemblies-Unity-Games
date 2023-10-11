using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
[Localize]
[ProtoInclude(13, typeof(EnemyData))]
[ProtoInclude(14, typeof(CharacterData))]
public abstract class CombatantData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class Statistics
	{
		private const int DEFAULT = 5;

		[ProtoMember(1)]
		[UIField(min = 0, max = 20)]
		[DefaultValue(5)]
		private int _offense = 5;

		[ProtoMember(2)]
		[UIField(min = 0, max = 20)]
		[DefaultValue(5)]
		private int _defense = 5;

		[ProtoMember(3)]
		[UIField(min = 1, max = 20)]
		[DefaultValue(5)]
		private int _health = 5;

		public int this[StatType stat] => stat switch
		{
			StatType.Offense => _offense, 
			StatType.Defense => _defense, 
			StatType.Health => _health, 
			StatType.NumberOfAttacks => 1, 
			StatType.ShieldRetention => 0, 
			_ => throw new ArgumentOutOfRangeException("stat", stat, null), 
		};

		public float GetExperienceValue()
		{
			return (float)(this[StatType.Health] + Math.Min(5, this[StatType.Offense]) + Math.Min(5, this[StatType.Defense]) + Math.Max(0, this[StatType.Offense] - 5) * 2 + Math.Max(0, this[StatType.Defense] - 5) * 2) * (1f / 3f);
		}
	}

	protected const string CAT_MAIN = "Main";

	protected const string CAT_COSMETIC = "Cosmetic";

	[ProtoMember(6)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, order = 1u, collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	protected LocalizedStringData _nameLocalized;

	[ProtoMember(2)]
	[UIField(order = 2u, collapse = UICollapseType.Hide)]
	[UICategory("Main")]
	protected Statistics _statistics;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(order = 3u, collapse = UICollapseType.Open)]
	[UIFieldCollectionItem(excludedValuesMethod = "_ExcludeTraits")]
	[UIDeepValueChange]
	[UIHideIf("_hideTraits")]
	[UICategory("Main")]
	protected List<DataRef<AbilityData>> _traits;

	[ProtoMember(4)]
	[UIField(order = 4u, collapse = UICollapseType.Open)]
	[UICategory("Cosmetic")]
	protected DataRef<EntityAudioData> _audio;

	[ProtoMember(5)]
	[UIField(order = 5u, collapse = UICollapseType.Open)]
	[UICategory("Cosmetic")]
	protected DataRef<CombatMediaData> _combatMedia;

	[ProtoMember(1)]
	protected string _name
	{
		get
		{
			return null;
		}
		set
		{
			if (_nameLocalized == null)
			{
				_nameLocalized = new LocalizedStringData(value);
			}
		}
	}

	[ProtoMember(15)]
	public string tags { get; set; }

	public string name => _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));

	public Statistics stats => _statistics ?? (_statistics = new Statistics());

	public IEnumerable<DataRef<AbilityData>> traits
	{
		get
		{
			if (_traits.IsNullOrEmpty())
			{
				yield break;
			}
			foreach (DataRef<AbilityData> trait in _traits)
			{
				yield return trait;
			}
		}
	}

	public EntityAudioData audio
	{
		get
		{
			if (!_audio)
			{
				return EntityAudioData.Default;
			}
			return _audio.data;
		}
	}

	public CombatMediaData combatMedia
	{
		get
		{
			if (!_combatMedia)
			{
				return CombatMediaData.Default;
			}
			return _combatMedia.data;
		}
	}

	protected virtual bool _hideTraits => false;

	private bool _audioSpecified => _audio;

	private bool _combatMediaSpecified => _combatMedia;

	public string GetTitle()
	{
		return name;
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return new List<string>(traits.Select((DataRef<AbilityData> abilityRef) => abilityRef.data.name));
	}

	public void PrepareDataForSave()
	{
		if (!_traits.IsNullOrEmpty())
		{
			_traits.RemoveAll((DataRef<AbilityData> d) => !d);
		}
	}

	public string GetSaveErrorMessage()
	{
		return name.IsNullOrEmpty().ToText("Please make sure to enter a name before saving.");
	}

	public void OnLoadValidation()
	{
	}

	protected bool _ExcludeTraits(DataRef<AbilityData> abilityRef)
	{
		if (abilityRef.data.type.IsTrait())
		{
			return abilityRef.data.characterClass.HasValue;
		}
		return true;
	}
}
