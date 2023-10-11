using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[UIField]
[UIDeepValidate]
public class ContentRefDefaults
{
	[ProtoContract]
	[UIField]
	public class Media
	{
		[ProtoContract]
		[UIField]
		public class AbilityRankMedia
		{
			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _normal;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _elite;

			[ProtoMember(3)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _legendary;

			public ProjectileMediaPack this[AbilityData.Rank rank] => rank switch
			{
				AbilityData.Rank.Elite => _elite, 
				AbilityData.Rank.Legendary => _legendary, 
				_ => _normal, 
			};

			private bool _normalSpecified => _normal;

			private bool _eliteSpecified => _elite;

			private bool _legendarySpecified => _legendary;
		}

		[ProtoContract]
		[UIField]
		public class LevelUpMedia
		{
			[ProtoMember(1)]
			[UIField(tooltip = "Should use HP as start & end points.")]
			[UIDeepValueChange]
			private ProjectileMediaPack _vialToPlant;

			[ProtoMember(2)]
			[UIField(tooltip = "Plays while xp is pouring into vial and half way point of fill has been reached.\nVial Center = Center, Middle Fill Line = Description")]
			[UIDeepValueChange]
			private ProjectileMediaPack _vialFillThresholdHalfway;

			[ProtoMember(3)]
			[UIField(tooltip = "Plays while xp is pouring into vial and it becomes full.\nVial Center = Center, Top Fill Line = Cost")]
			[UIDeepValueChange]
			private ProjectileMediaPack _vialFillComplete;

			[ProtoMember(4)]
			[UIField(tooltip = "Plays while xp is pouring into vial.\nTop = Name")]
			[UIDeepValueChange]
			private ProjectileMediaPack _vialGatherManaToTop;

			[ProtoMember(5)]
			[UIField(tooltip = "Plays while xp is pouring into vial.\nTop = Name, Liquid Surface = Image")]
			[UIDeepValueChange]
			private ProjectileMediaPack _vialManaPourFromTopToFillLine;

			[ProtoMember(6)]
			[UIField(tooltip = "Plays when card pack nut is clicked and pops open.\nCards = Image")]
			[UIDeepValueChange]
			private ProjectileMediaPack _cardPackOpen;

			public ProjectileMediaPack vialToPlant => _vialToPlant;

			public ProjectileMediaPack vialFillThresholdHalfway => _vialFillThresholdHalfway;

			public ProjectileMediaPack vialFillComplete => _vialFillComplete;

			public ProjectileMediaPack vialGatherManaToTop => _vialGatherManaToTop;

			public ProjectileMediaPack vialManaPourFromTopToFillLine => _vialManaPourFromTopToFillLine;

			public ProjectileMediaPack cardPackOpen => _cardPackOpen;

			private bool _vialToPlantSpecified => _vialToPlant;

			private bool _vialFillThresholdHalfwaySpecified => _vialFillThresholdHalfway;

			private bool _vialFillCompleteSpecified => _vialFillComplete;

			private bool _vialGatherManaToTopSpecified => _vialGatherManaToTop;

			private bool _vialManaPourFromTopToFillLineSpecified => _vialManaPourFromTopToFillLine;

			private bool _cardPackOpenSpecified => _cardPackOpen;
		}

		[ProtoContract]
		[UIField]
		public class BonusRankMedia
		{
			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _normal;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _elite;

			[ProtoMember(3)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _legendary;

			public ProjectileMediaPack this[AbilityData.Rank rank] => rank switch
			{
				AbilityData.Rank.Elite => _elite, 
				AbilityData.Rank.Legendary => _legendary, 
				_ => _normal, 
			};

			private bool _normalSpecified => _normal;

			private bool _eliteSpecified => _elite;

			private bool _legendarySpecified => _legendary;
		}

		[ProtoContract]
		[UIField]
		public class AdventureCompletionRankMedia
		{
			[ProtoContract]
			[UIField]
			public class MediaData
			{
				[ProtoMember(1)]
				[UIField(collapse = UICollapseType.Hide)]
				private CroppedImageRef _image = new CroppedImageRef(ImageCategoryType.Ability, AbilityData.Cosmetic.IMAGE_SIZE);

				[ProtoMember(2)]
				[UIField]
				private ProjectileMediaPack _clickMedia;

				public CroppedImageRef image => _image;

				public ProjectileMediaPack clickMedia => _clickMedia;

				public override string ToString()
				{
					return $"<b>Image:</b> {_image}, <b>Click Media:</b> {_clickMedia}";
				}
			}

			[ProtoMember(1)]
			[UIField]
			[UIFieldCollectionItem]
			[UIFieldKey(flexibleWidth = 1f)]
			[UIFieldValue(flexibleWidth = 4f)]
			private Dictionary<AdventureCompletionRank, MediaData> _media;

			public MediaData this[AdventureCompletionRank rank] => (_media ?? (_media = new Dictionary<AdventureCompletionRank, MediaData>())).GetValueOrDefault(rank);
		}

		[ProtoContract]
		[UIField]
		public class AdventureLevelUp
		{
			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankOne;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankTwo;

			public ProjectileMediaPack this[int rank]
			{
				get
				{
					ProjectileMediaPack projectileMediaPack = ((rank != 0) ? _rankTwo : _rankOne);
					ProjectileMediaPack projectileMediaPack2 = projectileMediaPack;
					if (projectileMediaPack2 == null || !projectileMediaPack2)
					{
						return _rankOne;
					}
					return projectileMediaPack2;
				}
			}
		}

		[ProtoContract]
		[UIField]
		public class Restore
		{
			[ProtoMember(1)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankOne;

			[ProtoMember(2)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankTwo;

			[ProtoMember(3)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankThree;

			[ProtoMember(4)]
			[UIField]
			[UIDeepValueChange]
			private ProjectileMediaPack _rankFour;

			public ProjectileMediaPack this[int rank]
			{
				get
				{
					ProjectileMediaPack projectileMediaPack = rank switch
					{
						0 => _rankOne, 
						1 => _rankTwo, 
						2 => _rankThree, 
						_ => _rankFour, 
					};
					if (projectileMediaPack == null || !projectileMediaPack)
					{
						return _rankOne;
					}
					return projectileMediaPack;
				}
			}
		}

		[ProtoMember(1)]
		[UIField]
		[UIDeepValueChange]
		private ProjectileMediaPack _topDeckSuccess;

		[ProtoMember(2)]
		[UIField]
		[UIDeepValueChange]
		private ProjectileMediaPack _topDeckFailure;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private ProjectileMediaPack _shieldLost;

		[ProtoMember(4)]
		[UIField]
		[UIDeepValueChange]
		private ProjectileMediaPack _safeAttack;

		[ProtoMember(5)]
		[UIField(tooltip = "This projectile media is used on startup to properly warmup all corresponding systems. It should have a full range of projectile system functionality.")]
		private DataRef<ProjectileMediaData> _warmupMedia;

		[ProtoMember(6)]
		[UIField]
		private CroppedImageRef _abilityImage = new CroppedImageRef(ImageCategoryType.Ability, AbilityData.Cosmetic.IMAGE_SIZE);

		[ProtoMember(7)]
		[UIField]
		private AbilityRankMedia _abilityRankMedia;

		[ProtoMember(8)]
		[UIField]
		private LevelUpMedia _levelUp;

		[ProtoMember(9)]
		[UIField]
		private BonusRankMedia _bonusRankMedia;

		[ProtoMember(10)]
		[UIField]
		private AdventureCompletionRankMedia _adventureCompletionRankMedia;

		[ProtoMember(11)]
		[UIField]
		private AdventureLevelUp _adventureLevelUp;

		[ProtoMember(12)]
		[UIField]
		private Restore _restore;

		public ProjectileMediaPack topDeckSuccess => _topDeckSuccess;

		public ProjectileMediaPack topDeckFailure => _topDeckFailure;

		public ProjectileMediaPack shieldLost => _shieldLost;

		public ProjectileMediaPack safeAttack => _safeAttack;

		public DataRef<ProjectileMediaData> warmupMedia => _warmupMedia;

		public CroppedImageRef abilityImage => _abilityImage;

		public AbilityRankMedia abilityRankMedia => _abilityRankMedia ?? (_abilityRankMedia = new AbilityRankMedia());

		public LevelUpMedia levelUp => _levelUp ?? (_levelUp = new LevelUpMedia());

		public BonusRankMedia bonusRankMedia => _bonusRankMedia ?? (_bonusRankMedia = new BonusRankMedia());

		public AdventureCompletionRankMedia adventureCompletionRankMedia => _adventureCompletionRankMedia ?? (_adventureCompletionRankMedia = new AdventureCompletionRankMedia());

		public AdventureLevelUp adventureLevelUp => _adventureLevelUp ?? (_adventureLevelUp = new AdventureLevelUp());

		public Restore restore => _restore ?? (_restore = new Restore());

		private bool _topDeckSuccessSpecified => _topDeckSuccess;

		private bool _topDeckFailureSpecified => _topDeckFailure;

		private bool _shieldLostSpecified => _shieldLost;

		private bool _safeAttackSpecified => _safeAttack;

		private bool _warmupMediaSpecified => _warmupMedia.ShouldSerialize();
	}

	[ProtoContract]
	[UIField]
	public class Lighting
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _adventure;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _adventureSelect;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _environment;

		[ProtoMember(4)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _deckCreation;

		[ProtoMember(5)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _victory;

		[ProtoMember(6)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _loss;

		[ProtoMember(7)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<LightingData> _deckCreationTransition;

		public DataRef<LightingData> adventure => _adventure;

		public DataRef<LightingData> adventureSelect => _adventureSelect;

		public DataRef<LightingData> environment => _environment;

		public DataRef<LightingData> deckCreation => _deckCreation;

		public DataRef<LightingData> victory => _victory;

		public DataRef<LightingData> loss => _loss;

		public DataRef<LightingData> deckCreationTransition => _deckCreationTransition;

		private bool _adventureSpecified => _adventure.ShouldSerialize();

		private bool _adventureSelectSpecified => _adventureSelect.ShouldSerialize();

		private bool _environmentSpecified => _environment.ShouldSerialize();

		private bool _deckCreationSpecified => _deckCreation.ShouldSerialize();

		private bool _victorySpecified => _victory.ShouldSerialize();

		private bool _lossSpecified => _loss.ShouldSerialize();

		private bool _deckCreationTransitionSpecified => _deckCreationTransition.ShouldSerialize();
	}

	[ProtoContract]
	[UIField]
	public class Audio
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<MusicData> _environmentMusic;

		[ProtoMember(2)]
		[UIField(filter = AudioCategoryType.Ambient, collapse = UICollapseType.Open, category = "Environment Ambient")]
		private AudioRef _environmentAmbient;

		[ProtoMember(3)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<MusicData> _victoryMusic;

		[ProtoMember(4)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<MusicData> _lossMusic;

		[ProtoMember(5, OverwriteList = true)]
		[UIField(maxCount = 0)]
		[UIFieldCollectionItem]
		private List<AdventureCard.SelectInstruction> _combatMusicInstructionsByDifficulty;

		public DataRef<MusicData> environmentMusic => _environmentMusic;

		public AudioRef environmentAmbient => _environmentAmbient;

		public DataRef<MusicData> victoryMusic => _victoryMusic;

		public DataRef<MusicData> lossMusic => _lossMusic;

		private bool _environmentMusicSpecified => _environmentMusic.ShouldSerialize();

		private bool _environmentAmbientSpecified => _environmentAmbient.ShouldSerialize();

		private bool _victoryMusicSpecified => _victoryMusic.ShouldSerialize();

		private bool _lossMusicSpecified => _lossMusic.ShouldSerialize();

		public AdventureCard.SelectInstruction GetCombatMusicInstruction(int difficulty)
		{
			return _combatMusicInstructionsByDifficulty?[Mathf.Clamp(difficulty, 0, _combatMusicInstructionsByDifficulty.Count - 1)];
		}
	}

	[ProtoContract]
	[UIField]
	public class Data
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<GameData> _startingGame;

		[ProtoMember(2)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<CharacterData> _startingCharacter;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(maxCount = 0)]
		[UIFieldCollectionItem]
		private Dictionary<DataRef<CharacterData>, DataRef<ProceduralNodeData>> _characterStories;

		[ProtoMember(5)]
		[UIField]
		private ProceduralNodePack _proceduralLevelUpNodeFallback;

		[ProtoMember(4, OverwriteList = true)]
		private Dictionary<uint, float> _abilityUnlockWeights;

		public DataRef<GameData> startingGame => _startingGame;

		public DataRef<CharacterData> startingCharacter => _startingCharacter;

		public ProceduralNodePack proceduralLevelUpNodeFallback => _proceduralLevelUpNodeFallback;

		private bool _startingGameSpecified => _startingGame.ShouldSerialize();

		private bool _startingCharacterSpecified => _startingCharacter.ShouldSerialize();

		public DataRef<ProceduralNodeData> GetCharacterStory(DataRef<CharacterData> character)
		{
			return _characterStories?.GetValueOrDefault(character);
		}

		public float GetAbilityUnlockWeight(DataRef<AbilityData> abilityRef)
		{
			return _abilityUnlockWeights?.GetValueOrDefault(abilityRef, 1f) ?? 1f;
		}

		[UIField]
		private void _CalculateAbilityUnlockWeights()
		{
			_abilityUnlockWeights = new Dictionary<uint, float>();
			float num = (float)(from d in AbilityData.GetAbilities()
				select d.data.description.Length).Average();
			Debug.Log($"Average Description Length: {num}");
			foreach (DataRef<AbilityData> ability in AbilityData.GetAbilities())
			{
				_abilityUnlockWeights[ability] = Mathf.Pow(num / (float)Mathf.Max(1, ability.data.description.Length), 1.5f);
				Debug.Log($"{ability.friendlyName}: {_abilityUnlockWeights[ability]}");
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class PlayerClassData
	{
		[ProtoContract]
		[UIField]
		public class AbilityWeights
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField(collapse = UICollapseType.Open, maxCount = 0)]
			[UIFieldCollectionItem]
			[UIFieldKey(excludedValuesMethod = "_ExcludeAbilityKey", dynamicInitMethod = "_InitAbilityKey", flexibleWidth = 1f)]
			[UIFieldValue(defaultValue = 1f, min = 0.1f, max = 10f, flexibleWidth = 1f)]
			private Dictionary<DataRef<AbilityData>, float> _abilityWeights;

			[ProtoMember(2)]
			[UIField]
			private bool _ignoreForPossibleTraits;

			public PlayerClass characterClass { get; set; }

			public bool ignoreForPossibleTraits => _ignoreForPossibleTraits;

			public double? GetWeight(DataRef<AbilityData> ability)
			{
				Dictionary<DataRef<AbilityData>, float> abilityWeights = _abilityWeights;
				if (abilityWeights == null || !abilityWeights.ContainsKey(ability))
				{
					return null;
				}
				return _abilityWeights[ability];
			}

			public IEnumerable<KeyValuePair<DataRef<AbilityData>, float>> GetWeights()
			{
				return _abilityWeights;
			}

			public override string ToString()
			{
				return $"{EnumUtil.FriendlyName(characterClass)} Trait Weights ({_abilityWeights?.Count ?? 0})" + _ignoreForPossibleTraits.ToText(" (Ignore For Possible Traits)".SizeIfNotEmpty());
			}

			private void _InitAbilityKey(UIFieldAttribute uiField)
			{
				uiField.defaultValue = DataRef<AbilityData>.All.FirstOrDefault((DataRef<AbilityData> a) => !_ExcludeAbilityKey(a));
			}

			private bool _ExcludeAbilityKey(DataRef<AbilityData> ability)
			{
				if ((bool)ability)
				{
					if (ability.data.characterClass == characterClass && ability.data.category == AbilityData.Category.Ability)
					{
						return _abilityWeights?.ContainsKey(ability) ?? false;
					}
					return true;
				}
				return false;
			}
		}

		[ProtoMember(1, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open, maxCount = 9)]
		[UIFieldCollectionItem]
		[UIFieldKey(excludedValuesMethod = "_ExcludeTraitKey", dynamicInitMethod = "_InitTraitKey", flexibleWidth = 1f)]
		[UIFieldValue(flexibleWidth = 2f)]
		private Dictionary<DataRef<AbilityData>, AbilityWeights> _traitWeights;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open, maxCount = 0)]
		[UIFieldCollectionItem]
		[UIFieldKey(excludedValuesMethod = "_ExcludeAbilityKey", dynamicInitMethod = "_InitAbilityKey")]
		[UIFieldValue(defaultValue = 1f, min = 0.1f, max = 10f)]
		private Dictionary<DataRef<AbilityData>, float> _abilityWeights;

		[ProtoMember(3, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open, maxCount = 0)]
		[UIFieldCollectionItem(excludedValuesMethod = "_ExcludeSurvivalAbility")]
		private List<DataRef<AbilityData>> _survivalAbilities;

		[ProtoMember(4, OverwriteList = true)]
		[UIField(collapse = UICollapseType.Open, maxCount = 0)]
		[UIFieldCollectionItem(excludedValuesMethod = "_ExcludeShortTermBuff")]
		private List<DataRef<AbilityData>> _shortTermBuffs;

		private readonly PlayerClass _class;

		public bool hasSurvivalAbilities => !_survivalAbilities.IsNullOrEmpty();

		private PlayerClassData()
		{
		}

		public PlayerClassData(PlayerClass playerClass)
		{
			_class = playerClass;
		}

		public double GetWeight(IEnumerable<DataRef<AbilityData>> activeTraits, DataRef<AbilityData> ability, HashSet<DataRef<AbilityData>> possibleTraits)
		{
			return activeTraits.Where((DataRef<AbilityData> t) => _traitWeights?.ContainsKey(t) ?? false).MaxOrDefault((DataRef<AbilityData> t) => _traitWeights[t].GetWeight(ability)) ?? ((double)(_abilityWeights?.GetValueOrDefault(ability, 1f) ?? 1f) * (possibleTraits.Where(delegate(DataRef<AbilityData> t)
			{
				AbilityWeights abilityWeights = _traitWeights?.GetValueOrDefault(t);
				return abilityWeights != null && !abilityWeights.ignoreForPossibleTraits;
			}).MaxOrDefault((DataRef<AbilityData> t) => _traitWeights[t].GetWeight(ability)) ?? 1.0));
		}

		public IEnumerable<DataRef<AbilityData>> SurvivalAbilities()
		{
			if (!hasSurvivalAbilities)
			{
				return null;
			}
			return _survivalAbilities;
		}

		public bool IsShortTermBuff(DataRef<AbilityData> ability)
		{
			if (ability.data.type == AbilityData.Type.Buff)
			{
				return _shortTermBuffs?.Contains(ability) ?? false;
			}
			return false;
		}

		private void OnValidateUI()
		{
			if (_traitWeights.IsNullOrEmpty())
			{
				return;
			}
			foreach (AbilityWeights value in _traitWeights.Values)
			{
				value.characterClass = _class;
			}
		}

		private void _InitTraitKey(UIFieldAttribute uiField)
		{
			uiField.defaultValue = DataRef<AbilityData>.All.FirstOrDefault((DataRef<AbilityData> a) => !_ExcludeTraitKey(a));
		}

		private bool _ExcludeTraitKey(DataRef<AbilityData> ability)
		{
			if ((bool)ability)
			{
				if (ability.data.characterClass == _class && ability.data.type.IsTrait())
				{
					return _traitWeights?.ContainsKey(ability) ?? false;
				}
				return true;
			}
			return false;
		}

		private void _InitAbilityKey(UIFieldAttribute uiField)
		{
			uiField.defaultValue = DataRef<AbilityData>.All.FirstOrDefault((DataRef<AbilityData> a) => !_ExcludeAbilityKey(a));
		}

		private bool _ExcludeAbilityKey(DataRef<AbilityData> ability)
		{
			if ((bool)ability)
			{
				if (ability.data.characterClass == _class && ability.data.category == AbilityData.Category.Ability)
				{
					return _abilityWeights?.ContainsKey(ability) ?? false;
				}
				return true;
			}
			return false;
		}

		private bool _ExcludeSurvivalAbility(DataRef<AbilityData> ability)
		{
			if ((bool)ability)
			{
				if (ability.data.characterClass == _class && ability.data.category == AbilityData.Category.Ability)
				{
					return _survivalAbilities?.Contains(ability) ?? false;
				}
				return true;
			}
			return false;
		}

		private bool _ExcludeShortTermBuff(DataRef<AbilityData> ability)
		{
			if ((bool)ability)
			{
				if (ability.data.characterClass == _class && ability.data.category == AbilityData.Category.Ability && ability.data.type == AbilityData.Type.Buff)
				{
					return _shortTermBuffs?.Contains(ability) ?? false;
				}
				return true;
			}
			return false;
		}

		[UIField(validateOnChange = true, tooltip = "Counter balance weights found in Trait Weights into Ability Weights.")]
		private void _CounterTraitWeights()
		{
			foreach (AbilityWeights value in _traitWeights.Values)
			{
				foreach (KeyValuePair<DataRef<AbilityData>, float> weight in value.GetWeights())
				{
					_abilityWeights[weight.Key] = 1f / Math.Max(0.01f, weight.Value);
				}
			}
		}
	}

	[ProtoContract]
	[UIField]
	public class SelectAbilityData
	{
		[ProtoContract]
		[UIField]
		public class SelectAbilityActions
		{
			[ProtoMember(1, OverwriteList = true)]
			[UIField(tooltip = "Actions should use <b>inherited targeting</b> as they will be fed targets by ability selection game steps.")]
			[UIFieldCollectionItem]
			[UIDeepValueChange]
			private List<AAbilityAction> _abilityActions;

			public void AppendSteps(GameState state, IEnumerable<GameStep> stepsToRunAfterActions, params Ability[] targetsToInherit)
			{
				ActionContext context = new ActionContext(state.player, null);
				AAction.Target.AAbility.Set set = new AAction.Target.AAbility.Set(targetsToInherit);
				state.stack.Append(new GameStepGrouper(Enumerable.Repeat(set.GetGameStep(context, new TargetAbilityAction(set)), 1).Concat(_abilityActions?.SelectMany((AAbilityAction action) => action.GetActGameSteps(context)) ?? Enumerable.Empty<GameStep>()).Concat(stepsToRunAfterActions ?? Enumerable.Empty<GameStep>())));
			}
		}

		[ProtoMember(1)]
		[UIField]
		private SelectAbilityActions _upgradeAbilityActions;

		[ProtoMember(2)]
		[UIField]
		private SelectAbilityActions _removeAbilityActions;

		[ProtoMember(3)]
		[UIField]
		private SelectAbilityActions _copyAbilityActions;

		[ProtoMember(4)]
		[UIField]
		private SelectAbilityActions _reduceCostAbilityActions;

		public SelectAbilityActions upgradeAbilityActions => _upgradeAbilityActions ?? (_upgradeAbilityActions = new SelectAbilityActions());

		public SelectAbilityActions removeAbilityActions => _removeAbilityActions ?? (_removeAbilityActions = new SelectAbilityActions());

		public SelectAbilityActions copyAbilityActions => _copyAbilityActions ?? (_copyAbilityActions = new SelectAbilityActions());

		public SelectAbilityActions reduceCostAbilityActions => _reduceCostAbilityActions ?? (_reduceCostAbilityActions = new SelectAbilityActions());
	}

	private const string FILENAME = "ContentRefDefaults";

	private const string EXTENSION = ".bytes";

	private const string CAT_MAIN = "Main";

	[ProtoMember(1)]
	[UIField]
	[UICategory("Media")]
	private Media _media;

	[ProtoMember(2)]
	[UIField]
	[UICategory("Lighting")]
	private Lighting _lighting;

	[ProtoMember(3)]
	[UIField]
	[UICategory("Audio")]
	private Audio _audio;

	[ProtoMember(4)]
	[UIField]
	[UICategory("Data")]
	private Data _data;

	[ProtoMember(5)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Warrior")]
	private PlayerClassData _warrior = new PlayerClassData(PlayerClass.Warrior);

	[ProtoMember(6)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Rogue")]
	private PlayerClassData _rogue = new PlayerClassData(PlayerClass.Rogue);

	[ProtoMember(7)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Mage")]
	private PlayerClassData _mage = new PlayerClassData(PlayerClass.Mage);

	[ProtoMember(8)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Hunter")]
	private PlayerClassData _hunter = new PlayerClassData(PlayerClass.Hunter);

	[ProtoMember(9)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Enchantress")]
	private PlayerClassData _enchantress = new PlayerClassData(PlayerClass.Enchantress);

	[ProtoMember(10)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Select Ability")]
	private SelectAbilityData _selectAbility;

	private bool _initialized;

	public static string LoadFilepath => IOUtil.Combine(IOUtil.Path_Generated_Resource_Data, "ContentRefDefaults");

	private static string SaveFilepath => IOUtil.Combine(IOUtil.DevSavePath, LoadFilepath) + ".bytes";

	public Media media => _media ?? (_media = new Media());

	public Lighting lighting => _lighting ?? (_lighting = new Lighting());

	public Audio audio => _audio ?? (_audio = new Audio());

	public Data data => _data ?? (_data = new Data());

	public SelectAbilityData selectAbility => _selectAbility ?? (_selectAbility = new SelectAbilityData());

	public ProjectileMediaPack this[TopDeckResult result] => result switch
	{
		TopDeckResult.Success => media.topDeckSuccess, 
		TopDeckResult.Failure => media.topDeckFailure, 
		_ => null, 
	};

	public PlayerClassData this[PlayerClass playerClass] => playerClass switch
	{
		PlayerClass.Warrior => _warrior, 
		PlayerClass.Rogue => _rogue, 
		PlayerClass.Mage => _mage, 
		PlayerClass.Hunter => _hunter, 
		PlayerClass.Enchantress => _enchantress, 
		_ => _warrior, 
	};

	[UIField]
	[UICategory("Main")]
	private void _Save()
	{
		Debug.Log("Saving with path: " + SaveFilepath);
		IOUtil.WriteBytes(this, SaveFilepath);
	}

	private void OnValidateUI()
	{
		if (!_initialized)
		{
			TextAsset textAsset = Resources.Load<TextAsset>(LoadFilepath);
			if ((bool)textAsset)
			{
				ProtoUtil.DeserializeInto(new MemoryStream(textAsset.bytes), this);
				_initialized = true;
			}
		}
	}
}
