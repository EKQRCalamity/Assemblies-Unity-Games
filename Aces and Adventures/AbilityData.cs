using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

[ProtoContract]
[UIField("Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null)]
[Localize]
public class AbilityData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class Activation
	{
		[ProtoMember(1)]
		[UIField(tooltip = "Determines when the ability can be used.")]
		[DefaultValue(CanActivateOn.OwnerTurn)]
		private CanActivateOn _canActivateOn = CanActivateOn.OwnerTurn;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(tooltip = "A list of specific game triggers which will make this ability temporarily usable.", collapse = UICollapseType.Open)]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		private List<AAction.Trigger> _canAlsoActivateWhen;

		[ProtoMember(3)]
		[UIField(tooltip = "Determines what resources must be consumed in order to activate the ability.", collapse = UICollapseType.Open)]
		[UIDeepValueChange]
		private AResourceCosts _cost;

		public CanActivateOn canActivateOn => _canActivateOn;

		public List<AAction.Trigger> canAlsoActivateWhen => _canAlsoActivateWhen ?? (_canAlsoActivateWhen = new List<AAction.Trigger>());

		public AResourceCosts cost => _cost ?? (_cost = new ResourceCosts());

		public bool this[CanActivateOn flag] => EnumUtil.HasFlag(canActivateOn, flag);
	}

	[ProtoContract]
	[UIField]
	public class Cosmetic
	{
		[ProtoContract]
		[UIField]
		public class CastMedia
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Defines what will be considered the \"Activator\" location of the projectile media.")]
			[UIHorizontalLayout("T")]
			[DefaultValue(CardTargetType.Ability)]
			private CardTargetType _activateFrom = CardTargetType.Ability;

			[ProtoMember(2)]
			[UIField(tooltip = "Defines what will be considered the \"Target\" for projectile media to be launched towards.")]
			[UIHorizontalLayout("T")]
			private CardTargetType _launchAt;

			[ProtoMember(4)]
			[UIField]
			[UIDeepValueChange]
			private Vocal _vocal;

			[ProtoMember(3)]
			[UIField(collapse = UICollapseType.Hide)]
			private ProjectileMediaPack _projectileMedia;

			public CardTargetType activator => _activateFrom;

			public CardTargetType target => _launchAt;

			public Vocal vocal => _vocal ?? (_vocal = new Vocal());

			public ProjectileMediaPack media => _projectileMedia;

			private bool _projectileMediaSpecified => _projectileMedia;

			private bool _vocalSpecified => _vocal;

			public static implicit operator bool(CastMedia castMedia)
			{
				if (castMedia != null)
				{
					if (!castMedia._projectileMedia)
					{
						return castMedia._vocal;
					}
					return true;
				}
				return false;
			}

			public override string ToString()
			{
				if (!this)
				{
					return "<i>Null</i>";
				}
				return string.Format("Vocal: {0}, Media: {1}", vocal, media ? ((string)media) : "<i>Null</i>");
			}
		}

		public static readonly Ushort2 IMAGE_SIZE = new Ushort2(425, 396);

		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Hide)]
		private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Ability, IMAGE_SIZE, ImageCategoryFlags.Adventure);

		[ProtoMember(2)]
		[UIField(tooltip = "Media that will play immediately after targets are selected for an ability, before act media plays.")]
		[UIDeepValueChange]
		private CastMedia _castMedia;

		public CroppedImageRef image
		{
			get
			{
				if (!_image)
				{
					return ContentRef.Defaults.media.abilityImage;
				}
				return _image;
			}
		}

		public CastMedia castMedia => _castMedia;

		public bool hasImage => _image;

		private bool _imageSpecified => _image;

		private bool _castMediaSpecified => _castMedia;
	}

	[ProtoContract]
	[UIField]
	public class Vocal
	{
		[ProtoContract(EnumPassthru = true)]
		public enum VocalType
		{
			None,
			Grunt,
			Attack,
			Friendly,
			Hostile,
			Custom
		}

		[ProtoMember(1)]
		[UIField(validateOnChange = true)]
		[UIHorizontalLayout("T", expandHeight = false)]
		private VocalType _type;

		[ProtoMember(2)]
		[UIField]
		[UIHideIf("_hideCustomSounds")]
		[UIDeepValueChange]
		private SoundPack _customSounds = new SoundPack(AudioCategoryType.AbilityVocal, AudioCategoryTypeFlags.Grunt | AudioCategoryTypeFlags.Attack | AudioCategoryTypeFlags.Hurt | AudioCategoryTypeFlags.CriticallyHurt | AudioCategoryTypeFlags.Death | AudioCategoryTypeFlags.TurnStart | AudioCategoryTypeFlags.HostileWords | AudioCategoryTypeFlags.FriendlyWords | AudioCategoryTypeFlags.AbilityVocal);

		[ProtoMember(3)]
		[UIField(min = 0, max = 100)]
		[UIHideIf("_hideCommon")]
		[UIHorizontalLayout("T", expandHeight = false)]
		private byte _waitRatio;

		public float waitRatio => (float)(int)_waitRatio * 0.01f;

		private bool _hideCommon => _type == VocalType.None;

		private bool _hideCustomSounds => _type != VocalType.Custom;

		private bool _customSoundsSpecified => _customSounds;

		private SoundPack _GetSoundPack(ACombatant combatant)
		{
			return _type switch
			{
				VocalType.None => null, 
				VocalType.Grunt => combatant.audio.grunt, 
				VocalType.Attack => combatant.audio.Attack(1), 
				VocalType.Friendly => combatant.audio.friendlyWords, 
				VocalType.Hostile => combatant.audio.hostileWords, 
				VocalType.Custom => _customSounds, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public VoiceSource Play(ACombatant combatant)
		{
			return VoiceManager.Instance.Play(combatant.view.transform, _GetSoundPack(combatant), interrupt: true);
		}

		public static implicit operator bool(Vocal vocal)
		{
			if (vocal != null && vocal._type != 0)
			{
				if (vocal._type == VocalType.Custom)
				{
					return vocal._customSounds;
				}
				return true;
			}
			return false;
		}

		public static implicit operator string(Vocal vocal)
		{
			return vocal?.ToString() ?? "None";
		}

		public override string ToString()
		{
			if (_type == VocalType.Custom)
			{
				return $"[Custom: {_customSounds}]" + (_waitRatio > 0).ToText($" <size=66%>(Wait: {_waitRatio}%)</size>");
			}
			return EnumUtil.FriendlyName(_type);
		}
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Type
	{
		[UITooltip("An ability with an instant or very short term effect.")]
		Standard,
		[UITooltip("Applied to the Player for a specified duration.")]
		Buff,
		[UITooltip("Applied to an Enemy for a specified duration.")]
		Debuff,
		[UITooltip("Placed into turn order queue. Can have positional and turn start based actions.")]
		Summon,
		[UITooltip("Applied to the Player as a sort of buff so long as in their Ability Hand.")]
		Passive,
		[UITooltip("A constant effect applied to the owner.<i>Targeting logic used for traits should be self only.</i>")]
		Trait,
		[UITooltip("Like a trait, but instead of being applied constantly, it triggers based on specified reactions.\n<i>Targeting logic used for these traits should not require a manual selection of a target.</i>")]
		TriggeredTrait
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Rank
	{
		[UITooltip("Max of 4 in a deck. Most common to get from random card packs.")]
		Normal,
		[UITooltip("Max of 2 in a deck. Rare to get from random card packs.")]
		Elite,
		[UITooltip("Max of 1 in a deck. Very rare to get from random card packs.")]
		Legendary
	}

	[ProtoContract(EnumPassthru = true)]
	public enum Category
	{
		[UITooltip("This ability can be placed into custom ability decks for the selected class.")]
		Ability,
		[UITooltip("This ability will be one of the choices for the Hero Abilities of the selected class.")]
		HeroAbility,
		[UITooltip("This ability will be one of the choices for the Level 1 Upgrades of the selected class.")]
		Level1Trait,
		[UITooltip("This ability will be one of the choices for the Level 2 Upgrades of the selected class.")]
		Level2Trait,
		[UITooltip("This ability will be one of the choices for the Level 3 Upgrades of the selected class.")]
		Level3Trait,
		[UITooltip("This ability will be unlocked by leveling up a character (outside of an adventure) and will be usable once per adventure.")]
		TrumpCard
	}

	private const string CAT_MAIN = "Main";

	private const string CAT_ACTIVATE = "Activation";

	private const string CAT_COSMETIC = "Cosmetic";

	private const string CAT_ACTIONS = "Actions";

	private const string CAT_KEYWORDS = "Keywords";

	private static Dictionary<PlayerClass, List<DataRef<AbilityData>>> _abilitiesByClass;

	private static List<DataRef<AbilityData>> _abilities;

	private static Dictionary<PlayerClass, Dictionary<Category, Dictionary<Rank, DataRef<AbilityData>>>> _abilitiesByCategoryAndRank;

	[ProtoMember(14)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Name of the ability.", collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(2)]
	[UIField(tooltip = "Determines how many copies of this ability can be in a deck, as well as the rarity.")]
	[UIHorizontalLayout("Name", preferredWidth = 1f, flexibleWidth = 999f)]
	[UICategory("Main")]
	private Rank _rank;

	[ProtoMember(3)]
	[UIField("Ability Type", 0u, null, null, null, null, null, null, false, null, 5, false, null, validateOnChange = true, tooltip = "Determines the type of the ability, granting it unique properties.")]
	[UIHorizontalLayout("Name", preferredWidth = 1f, flexibleWidth = 999f)]
	[UICategory("Main")]
	private Type _type;

	[ProtoMember(20)]
	[UIField(tooltip = "If true, trait cannot be negated by standard means.")]
	[UIHorizontalLayout("Name", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 170f)]
	[UICategory("Main")]
	[UIHideIf("_hidePermanent")]
	private bool _permanent;

	[ProtoMember(21)]
	[UIField(tooltip = "If true, ability cannot be resisted or nullified.")]
	[UIHorizontalLayout("Name", preferredWidth = 1f, flexibleWidth = 0f, minWidth = 170f)]
	[UICategory("Main")]
	private bool _irresistible;

	[ProtoMember(4)]
	[UIField(validateOnChange = true, tooltip = "Determines which player class can use this ability.")]
	[UIHorizontalLayout("Class", preferredWidth = 1f)]
	[UICategory("Main")]
	private PlayerClass? _class;

	[ProtoMember(5)]
	[UIField(tooltip = "Determines where the ability belongs.")]
	[UIHorizontalLayout("Class", preferredWidth = 1f)]
	[UIHideIf("_hideCategory")]
	[UICategory("Main")]
	protected Category _category;

	[ProtoMember(15)]
	[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UICategory("Main")]
	[UIDeepValueChange]
	private LocalizedStringData _descriptionLocalized;

	[ProtoMember(6)]
	[UIField(collapse = UICollapseType.Hide)]
	[UIHideIf("_hideActivation")]
	[UICategory("Activation")]
	private Activation _activation;

	[ProtoMember(11)]
	[UIField(tooltip = "By default, the first action of an ability must find a target in order for it to be considered useable.\nToggle this option on to check all actions for an initial target.")]
	[UICategory("Actions")]
	private bool _checkAllActionsForInitialTargeting;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(tooltip = "Determines what the ability does when activated.", collapse = UICollapseType.Open, maxCount = 10)]
	[UIFieldCollectionItem(filterMethod = "_FilterActionTypes")]
	[UIDeepValueChange]
	[UICategory("Actions")]
	private List<AAction> _actions;

	[ProtoMember(13)]
	[UIField(tooltip = "This ability will not be unlocked until all copies of selected ability have been acquired.")]
	[UICategory("Main")]
	private DataRef<AbilityData> _upgradeOf;

	[ProtoMember(9)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Cosmetic")]
	private Cosmetic _cosmetic;

	[ProtoMember(10, OverwriteList = true)]
	[UIField]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UIMargin(24f, false)]
	[UIHeader("Advanced")]
	[UIHideIf("_hideRefreshTargetsWhen")]
	[UICategory("Actions")]
	private List<AAction.Duration> _refreshTargetsWhen;

	[ProtoMember(12)]
	[UIField(min = -5, max = 5, tooltip = "Determines how much additional experience an enemy with this trait rewards.")]
	[DefaultValue(1)]
	[UIHorizontalLayout("Class", preferredWidth = 1f)]
	[UIHideIf("_hideExperience")]
	[UICategory("Main")]
	private float _experience = 1f;

	[ProtoMember(16)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Keywords")]
	private HashSet<AbilityKeyword> _includeKeywords;

	[ProtoMember(17)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Keywords")]
	private HashSet<AbilityKeyword> _excludeKeywords;

	[ProtoMember(18)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Keywords")]
	private HashSet<DataRef<AbilityData>> _excludeTraits;

	[ProtoMember(19)]
	[UIField(collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Keywords")]
	private HashSet<DataRef<AbilityData>> _includedTraits;

	[ProtoMember(1)]
	private string _name
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

	[ProtoMember(8)]
	private string _description
	{
		get
		{
			return null;
		}
		set
		{
			if (_descriptionLocalized == null)
			{
				_descriptionLocalized = new LocalizedStringData(value);
			}
		}
	}

	[ProtoMember(128)]
	public string tags { get; set; }

	public string name => nameLocalized;

	public LocalizedStringData nameLocalized => _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));

	public string description => _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));

	public Rank rank => _rank;

	public Activation activation => _activation ?? (_activation = new Activation());

	public List<AAction> actions => _actions ?? (_actions = new List<AAction>());

	public Type type => _type;

	public bool permanent => _permanent;

	public bool irresistible => _irresistible;

	public bool canBeResisted
	{
		get
		{
			if (!irresistible)
			{
				return !registersRefreshTargets;
			}
			return false;
		}
	}

	public PlayerClass? characterClass => _class;

	public Category category => _category;

	public Cosmetic cosmetic => _cosmetic ?? (_cosmetic = new Cosmetic());

	public bool canRegisterRefreshTargets => type switch
	{
		Type.Buff => true, 
		Type.Debuff => true, 
		Type.Summon => true, 
		Type.Trait => true, 
		_ => false, 
	};

	public bool registersRefreshTargets
	{
		get
		{
			if (canRegisterRefreshTargets)
			{
				return hasRegisterRefreshTargets;
			}
			return false;
		}
	}

	public bool hasRegisterRefreshTargets => !_refreshTargetsWhen.IsNullOrEmpty();

	public List<AAction.Duration> refreshTargetsWhen => _refreshTargetsWhen ?? (_refreshTargetsWhen = new List<AAction.Duration>());

	public bool hasReaction => activation.canAlsoActivateWhen.Count > 0;

	public bool onlyAvailableDuringReaction
	{
		get
		{
			if (activation.canActivateOn == (CanActivateOn)0)
			{
				return activation.canAlsoActivateWhen.Count > 0;
			}
			return false;
		}
	}

	public bool onlyAvailableDuringDefense
	{
		get
		{
			if (activation.canActivateOn == CanActivateOn.OwnerPrepareDefense)
			{
				return activation.canAlsoActivateWhen.Count == 0;
			}
			return false;
		}
	}

	public bool checkAllActionsForInitialTargeting => _checkAllActionsForInitialTargeting;

	public float experience => _experience;

	public DataRef<AbilityData> upgradeOf => _upgradeOf;

	private bool _hideCategory => !_class.HasValue;

	private bool _hideActivation
	{
		get
		{
			if (_type != Type.Passive)
			{
				return _type == Type.Trait;
			}
			return true;
		}
	}

	private bool _hideRefreshTargetsWhen => !canRegisterRefreshTargets;

	private bool _hideExperience
	{
		get
		{
			if (!_class.HasValue)
			{
				return !_type.IsTrait();
			}
			return true;
		}
	}

	private bool _hideCopyDescriptionVariables
	{
		get
		{
			if ((bool)_upgradeOf)
			{
				return !_descriptionLocalized;
			}
			return true;
		}
	}

	private bool _hideCopyName
	{
		get
		{
			if ((bool)_upgradeOf)
			{
				return !_nameLocalized;
			}
			return true;
		}
	}

	private bool _hidePermanent => !_type.IsTrait();

	private bool _hideCopyActions => !_upgradeOf;

	private bool _upgradeOfSpecified => _upgradeOf.ShouldSerialize();

	public static IEnumerable<DataRef<AbilityData>> GetAbilities(PlayerClass characterClass)
	{
		return (_abilitiesByClass ?? (_abilitiesByClass = new Dictionary<PlayerClass, List<DataRef<AbilityData>>>())).GetValueOrDefault(characterClass) ?? (_abilitiesByClass[characterClass] = new List<DataRef<AbilityData>>(from a in GetAbilities()
			where a.data.characterClass == characterClass && a.data.category == Category.Ability
			select a));
	}

	public static IEnumerable<DataRef<AbilityData>> GetAbilities()
	{
		return _abilities ?? (_abilities = new List<DataRef<AbilityData>>(DataRef<AbilityData>.All.Where((DataRef<AbilityData> a) => a.data.category == Category.Ability && a.data.characterClass.HasValue)));
	}

	public static DataRef<AbilityData> GetAbility(PlayerClass characterClass, Category category, Rank rank)
	{
		if (_abilitiesByCategoryAndRank == null)
		{
			_abilitiesByCategoryAndRank = new Dictionary<PlayerClass, Dictionary<Category, Dictionary<Rank, DataRef<AbilityData>>>>();
			PlayerClass[] values = EnumUtil<PlayerClass>.Values;
			foreach (PlayerClass key in values)
			{
				_abilitiesByCategoryAndRank[key] = new Dictionary<Category, Dictionary<Rank, DataRef<AbilityData>>>();
			}
			foreach (DataRef<AbilityData> item in DataRef<AbilityData>.All)
			{
				if (item.data.category != 0)
				{
					PlayerClass? playerClass = item.data.characterClass;
					if (playerClass.HasValue)
					{
						PlayerClass valueOrDefault = playerClass.GetValueOrDefault();
						Dictionary<Category, Dictionary<Rank, DataRef<AbilityData>>> dictionary = _abilitiesByCategoryAndRank[valueOrDefault];
						(dictionary.GetValueOrDefault(item.data.category) ?? (dictionary[item.data.category] = new Dictionary<Rank, DataRef<AbilityData>>()))[item.data.rank] = item;
					}
				}
			}
		}
		Dictionary<Category, Dictionary<Rank, DataRef<AbilityData>>> valueOrDefault2 = _abilitiesByCategoryAndRank.GetValueOrDefault(characterClass);
		if (valueOrDefault2 != null)
		{
			Dictionary<Rank, DataRef<AbilityData>> valueOrDefault3 = valueOrDefault2.GetValueOrDefault(category);
			if (valueOrDefault3 != null)
			{
				return valueOrDefault3.GetValueOrDefault(rank);
			}
		}
		return null;
	}

	public static PoolKeepItemDictionaryHandle<Category, PoolKeepItemListHandle<DataRef<AbilityData>>> GetNonStandardAbilitiesByCategory(PlayerClass characterClass)
	{
		PoolKeepItemDictionaryHandle<Category, PoolKeepItemListHandle<DataRef<AbilityData>>> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<Category, PoolKeepItemListHandle<DataRef<AbilityData>>>();
		Category[] values = EnumUtil<Category>.Values;
		foreach (Category category in values)
		{
			if (category == Category.Ability)
			{
				continue;
			}
			poolKeepItemDictionaryHandle.Add(category, Pools.UseKeepItemList<DataRef<AbilityData>>());
			Rank[] values2 = EnumUtil<Rank>.Values;
			foreach (Rank rank in values2)
			{
				DataRef<AbilityData> ability = GetAbility(characterClass, category, rank);
				if (ability != null)
				{
					poolKeepItemDictionaryHandle[category].Add(ability);
				}
			}
		}
		return poolKeepItemDictionaryHandle;
	}

	public IEnumerable<string> GetDisplayedTags()
	{
		foreach (AbilityKeyword tagKeyword in GetTagKeywords())
		{
			yield return tagKeyword.GetTag();
		}
	}

	public string GetDisplayedTagString()
	{
		return GetDisplayedTags().ToStringSmart(" ");
	}

	private string _GetDisplayedTagStringEnglish()
	{
		return (from t in GetTagKeywords()
			select t.GetTag(LocalizationSettings.ProjectLocale)).ToStringSmart(" ");
	}

	public string GetLocalizedTagString()
	{
		return LocalizationSettings.StringDatabase.GetTable("Message")?.GetEntry(GenerateAbilityTagEntryMeta.GetKey(_GetDisplayedTagStringEnglish()))?.Value ?? GetDisplayedTagString();
	}

	public IEnumerable<AbilityKeyword> GetTagKeywords()
	{
		if (activation.canAlsoActivateWhen.Count > 0)
		{
			if (EnumUtil.HasAllFlags(activation.canActivateOn))
			{
				yield return AbilityKeyword.AbilityTagAsync;
			}
			else if (activation[CanActivateOn.OwnerPrepareDefense])
			{
				yield return AbilityKeyword.AbilityTagDefense;
			}
			if (!type.IsTrait())
			{
				yield return AbilityKeyword.AbilityTagReaction;
			}
		}
		else if (activation.canActivateOn > CanActivateOn.OwnerTurn)
		{
			AbilityKeyword? abilityKeyword = activation.canActivateOn switch
			{
				CanActivateOn.OwnerPrepareAttack => AbilityKeyword.AbilityTagAttack, 
				CanActivateOn.OwnerPrepareDefense => AbilityKeyword.AbilityTagDefense, 
				CanActivateOn.OwnerTurn | CanActivateOn.OwnerPrepareDefense => AbilityKeyword.AbilityTagDefense, 
				CanActivateOn.OwnerPrepareAttack | CanActivateOn.OwnerPrepareDefense => AbilityKeyword.AbilityTagCombat, 
				CanActivateOn.OwnerPrepareAttack | CanActivateOn.OwnerPrepareDefense | CanActivateOn.OwnerReaction => AbilityKeyword.AbilityTagAsync, 
				CanActivateOn.OwnerTurn | CanActivateOn.OwnerPrepareAttack | CanActivateOn.OwnerPrepareDefense | CanActivateOn.OwnerReaction => AbilityKeyword.AbilityTagAsync, 
				_ => null, 
			};
			if (abilityKeyword.HasValue)
			{
				yield return abilityKeyword.Value;
			}
			else if (activation.canAlsoActivateWhen.Count > 0)
			{
				yield return AbilityKeyword.AbilityTagReaction;
			}
			if (!EnumUtil.HasFlag(activation.canActivateOn, CanActivateOn.OwnerTurn))
			{
				yield return AbilityKeyword.AbilityTagOnly;
			}
		}
		if (category != 0)
		{
			yield return (AbilityKeyword)(12 + (category - 1));
		}
		else if (type != 0)
		{
			if (type.IsTrait())
			{
				yield return AbilityKeyword.AbilityTagTrait;
			}
			else
			{
				yield return (AbilityKeyword)(17 + (type - 1));
			}
		}
	}

	public PoolKeepItemHashSetHandle<AbilityKeyword> GetSearchFilterKeywords()
	{
		PoolKeepItemHashSetHandle<AbilityKeyword> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<AbilityKeyword>();
		foreach (AbilityKeyword tagKeyword in GetTagKeywords())
		{
			if (tagKeyword.GetSearchFilter().HasVisibleCharacter())
			{
				poolKeepItemHashSetHandle.Add(tagKeyword);
			}
		}
		foreach (AAction action in actions)
		{
			foreach (AbilityKeyword keyword in action.GetKeywords(this))
			{
				if (keyword.GetSearchFilter().HasVisibleCharacter())
				{
					poolKeepItemHashSetHandle.Add(keyword);
				}
			}
		}
		if ((bool)upgradeOf)
		{
			poolKeepItemHashSetHandle.Add(AbilityKeyword.Upgrade);
			poolKeepItemHashSetHandle.Add(AbilityKeyword.UpgradePlus);
		}
		return poolKeepItemHashSetHandle;
	}

	public IEnumerable<AbilityKeyword> IncludedKeywords()
	{
		IEnumerable<AbilityKeyword> includeKeywords = _includeKeywords;
		return includeKeywords ?? Enumerable.Empty<AbilityKeyword>();
	}

	public IEnumerable<AbilityKeyword> ExcludedKeywords()
	{
		IEnumerable<AbilityKeyword> excludeKeywords = _excludeKeywords;
		return excludeKeywords ?? Enumerable.Empty<AbilityKeyword>();
	}

	public IEnumerable<DataRef<AbilityData>> ExcludedTraits()
	{
		IEnumerable<DataRef<AbilityData>> excludeTraits = _excludeTraits;
		return excludeTraits ?? Enumerable.Empty<DataRef<AbilityData>>();
	}

	public IEnumerable<DataRef<AbilityData>> IncludedTraits()
	{
		IEnumerable<DataRef<AbilityData>> includedTraits = _includedTraits;
		return includedTraits ?? Enumerable.Empty<DataRef<AbilityData>>();
	}

	public IEnumerable<DataRef<AbilityData>> GetDowngrades()
	{
		DataRef<AbilityData> downgrade = upgradeOf;
		while ((bool)downgrade)
		{
			yield return downgrade;
			downgrade = downgrade.data.upgradeOf;
		}
	}

	public IEnumerable<string> GetTooltips(Ability ability = null)
	{
		using PoolKeepItemHashSetHandle<DataRef<AbilityData>> traitHash = Pools.UseKeepItemHashSet(ExcludedTraits());
		using PoolKeepItemHashSetHandle<AbilityKeyword> keywordHash = Pools.UseKeepItemHashSet(ExcludedKeywords());
		foreach (AbilityKeyword item in ability?.GetTagKeywords() ?? GetTagKeywords())
		{
			if (keywordHash.Add(item))
			{
				yield return item.GetTooltip();
			}
		}
		foreach (AAction.Trigger item2 in activation.canAlsoActivateWhen)
		{
			foreach (AbilityKeyword keyword in item2.GetKeywords())
			{
				if (keywordHash.Add(keyword))
				{
					yield return keyword.GetTooltip();
				}
			}
		}
		foreach (AAction action in actions)
		{
			foreach (AbilityKeyword keyword2 in action.GetKeywords(this))
			{
				if (keywordHash.Add(keyword2))
				{
					yield return keyword2.GetTooltip();
				}
			}
			if (action is AddTraitAction addTraitAction)
			{
				DataRef<AbilityData> trait = addTraitAction.trait;
				if (trait != null && traitHash.Add(trait))
				{
					yield return trait.data.description;
				}
			}
		}
		foreach (AbilityKeyword item3 in IncludedKeywords())
		{
			if (keywordHash.Add(item3))
			{
				yield return item3.GetTooltip();
			}
		}
		foreach (DataRef<AbilityData> item4 in IncludedTraits())
		{
			if (traitHash.Add(item4))
			{
				yield return item4.data.description;
			}
		}
	}

	private bool _FilterActionTypes(System.Type actionType)
	{
		return actionType == typeof(TargetStoneAction);
	}

	[UIField(validateOnChange = true, tooltip = "Copies variables from Upgrade Of ability as top level variables and also copies description text.\n<i>Hold Alt to use root ability.</i>")]
	[UIHideIf("_hideCopyDescriptionVariables")]
	[UICategory("Main")]
	[UIHorizontalLayout("Copy Vars")]
	private void _CopyDescriptionVariablesFromBaseAbility()
	{
		AbilityData abilityData = (InputManager.I[KeyModifiers.Alt] ? _upgradeOf.BaseAbilityRef().data : _upgradeOf.data);
		foreach (KeyValuePair<LocalizedVariableName, LocalizedStringData.AVariable> variable in abilityData._descriptionLocalized.variables.variables)
		{
			_descriptionLocalized.variables[variable.Key] = ProtoUtil.Clone(variable.Value);
		}
		_descriptionLocalized.rawText = ProtoUtil.Clone(abilityData._descriptionLocalized.rawText);
	}

	[UIField(validateOnChange = true, tooltip = "Copies variables from Upgrade Of ability into Description sub variable and sets description to {}.\n<i>Hold Alt to use root ability.</i>")]
	[UIHideIf("_hideCopyDescriptionVariables")]
	[UICategory("Main")]
	[UIHorizontalLayout("Copy Vars")]
	private void _CopyDescriptionVariablesFromBaseAbilitySimple()
	{
		AbilityData abilityData = (InputManager.I[KeyModifiers.Alt] ? _upgradeOf.BaseAbilityRef().data : _upgradeOf.data);
		_descriptionLocalized.variables[LocalizedVariableName.Description] = new LocalizedStringData.AVariable.LocalizedStringVariable(abilityData._descriptionLocalized.id, ProtoUtil.Clone(abilityData._descriptionLocalized.variables));
		_descriptionLocalized.rawText = "{}";
	}

	[UIField(validateOnChange = true, order = 1u)]
	[UICategory("Main")]
	[UIHideIf("_hideCopyName")]
	private void _CopyAbilityNameFromBaseAbility()
	{
		_nameLocalized.rawText = "{}";
		_nameLocalized.variables.variables.Clear();
		LocalizedStringData.AVariable.LocalizedStringVariable value = new LocalizedStringData.AVariable.LocalizedStringVariable(MessageData.Instance.ability.nameTableEntryId, new LocalizedStringData.VariableGroup
		{
			variables = 
			{
				{
					LocalizedVariableName.Name,
					(LocalizedStringData.AVariable)new LocalizedStringData.AVariable.AbilityNameVariable(_upgradeOf.BaseAbilityRef())
				},
				{
					LocalizedVariableName.Level,
					(LocalizedStringData.AVariable)new LocalizedStringData.AVariable.IntVariable(rank - _upgradeOf.BaseAbilityRef().data.rank)
				}
			}
		});
		_nameLocalized.variables[LocalizedVariableName.Var] = value;
	}

	[UIField]
	[UICategory("Main")]
	private void _CreateUpgradedVersionOfThisAbility()
	{
		ContentRef contentRef = DataRefControl.ActiveControl?.dataRef;
		DataRef<AbilityData> abilityRef = contentRef as DataRef<AbilityData>;
		if (abilityRef == null || !contentRef.hasSavedContent)
		{
			return;
		}
		UIUtil.CreatePopup("Create Upgrade of [" + abilityRef.data.GetTitle() + "] Ability", UIUtil.CreateMessageBox("Would you like to create an upgraded version of [" + abilityRef.data.GetTitle() + "] ability?", TextAlignmentOptions.Center, 32, 1600), null, parent: DataRefControl.ActiveControl.transform, buttons: new string[2] { "Create", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (!(s != "Create"))
			{
				AbilityData abilityData = ProtoUtil.Clone(abilityRef.data);
				abilityData._upgradeOf = abilityRef;
				abilityData._rank = EnumUtil.Next(rank);
				LocalizationUtil.ClearIds(abilityData);
				string title2 = $"{abilityRef.BaseAbilityRef().name} +{abilityData.rank - abilityRef.BaseAbilityRef().data.rank}";
				abilityData._nameLocalized.rawText = title2;
				DataRef<AbilityData> newAbilityDataRef = new DataRef<AbilityData>(abilityData);
				newAbilityDataRef.SaveFromUIWithoutValidation(abilityData);
				UIUtil.BeginProcessJob(DataRefControl.ActiveControl.transform, null, Department.UI).Then().DoProcess(Job.WaitForDepartment(Department.Content))
					.Then()
					.DoProcess(Job.WaitForOneFrame())
					.Then()
					.Do(delegate
					{
						ReflectionUtil.LocalizeDataRef(newAbilityDataRef, abilityData, title2);
						abilityData._CopyAbilityNameFromBaseAbility();
						newAbilityDataRef.SaveFromUIWithoutValidation(abilityData);
					})
					.Then()
					.DoProcess(Job.WaitForDepartment(Department.Content))
					.Then()
					.Do(UIUtil.EndProcess)
					.Then()
					.Do(delegate
					{
						DataRefControl.ActiveControl.SetDataRef(newAbilityDataRef);
					});
			}
		});
	}

	[UIField(validateOnChange = true)]
	[UICategory("Actions")]
	[UIHeader("Functions")]
	[UIHideIf("_hideCopyActions")]
	private void _CopyActionsFromBaseAbility()
	{
		_checkAllActionsForInitialTargeting = upgradeOf.data.checkAllActionsForInitialTargeting;
		_actions = ProtoUtil.Clone(upgradeOf.data.actions);
	}

	[UIField]
	[UICategory("Keywords")]
	private void _DebugTooltips()
	{
		foreach (string tooltip in GetTooltips())
		{
			Debug.Log(tooltip);
		}
	}

	public string GetTitle()
	{
		return name;
	}

	public string GetAutomatedDescription()
	{
		return description;
	}

	public List<string> GetAutomatedTags()
	{
		List<string> obj = new List<string>
		{
			description.RemoveRichText(),
			GetLocalizedTagString()
		};
		obj.AddMany(activation.cost.additionalCosts.GetSearchStrings());
		return obj;
	}

	public void PrepareDataForSave()
	{
		if (_hideRefreshTargetsWhen)
		{
			_refreshTargetsWhen = null;
		}
		if (_hideActivation)
		{
			_activation = null;
		}
		if (_hideCategory)
		{
			_category = Category.Ability;
		}
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
