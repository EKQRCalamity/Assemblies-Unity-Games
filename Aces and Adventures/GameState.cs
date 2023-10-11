using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ProtoBuf;
using UnityEngine;

[ProtoContract(SkipConstructor = true)]
public class GameState
{
	[ProtoContract]
	public class Parameters
	{
		[ProtoMember(1)]
		private int _startingAbilityCount;

		[ProtoMember(2)]
		private int _mulliganCount;

		[ProtoMember(3)]
		private int _jokerCount;

		[ProtoMember(4)]
		private int _restHealthGain;

		[ProtoMember(5)]
		private int _restAbilityDrawCount;

		[ProtoMember(6)]
		private int _restMulliganEnabled;

		[ProtoMember(7)]
		private int _levelUpHealAmount;

		[ProtoMember(8)]
		private int _initialLevelUpAmount;

		[ProtoMember(9)]
		private Flags _flags;

		[ProtoMember(10)]
		private int _restMulliganAbilityEnabled;

		[ProtoMember(11)]
		private int _viewMapNodes;

		public int startingAbilityCount
		{
			get
			{
				return _startingAbilityCount;
			}
			private set
			{
				_startingAbilityCount = value;
			}
		}

		public int mulliganCount
		{
			get
			{
				return _mulliganCount;
			}
			private set
			{
				_mulliganCount = value;
			}
		}

		public int jokerCount
		{
			get
			{
				return _jokerCount;
			}
			private set
			{
				_jokerCount = value;
			}
		}

		public int restHealthGain
		{
			get
			{
				return _restHealthGain;
			}
			private set
			{
				_restHealthGain = value;
			}
		}

		public int restAbilityDrawCount
		{
			get
			{
				return _restAbilityDrawCount;
			}
			private set
			{
				_restAbilityDrawCount = value;
			}
		}

		public bool restMulliganEnabled => _restMulliganEnabled > 0;

		public bool restMulliganAbilityEnabled => _restMulliganAbilityEnabled > 0;

		public int levelUpHealAmount
		{
			get
			{
				return _levelUpHealAmount;
			}
			private set
			{
				_levelUpHealAmount = value;
			}
		}

		public int initialLevelUpAmount
		{
			get
			{
				return _initialLevelUpAmount;
			}
			private set
			{
				_initialLevelUpAmount = value;
			}
		}

		public int numberOfRestUpgrades => Math.Sign(_restHealthGain) + Math.Sign(_restAbilityDrawCount) + Math.Sign(_restMulliganEnabled);

		public int numberOfLevelUpgrades => Math.Sign(_levelUpHealAmount);

		public bool adventureStarted
		{
			get
			{
				return EnumUtil.HasFlag(_flags, Flags.AdventureStarted);
			}
			set
			{
				EnumUtil.SetFlag(ref _flags, Flags.AdventureStarted, value);
			}
		}

		public bool adventureEnded
		{
			get
			{
				return EnumUtil.HasFlag(_flags, Flags.AdventureEnded);
			}
			set
			{
				EnumUtil.SetFlag(ref _flags, Flags.AdventureEnded, value);
			}
		}

		public bool adventureBeganInitialize
		{
			get
			{
				return EnumUtil.HasFlag(_flags, Flags.AdventureBeganInitialize);
			}
			set
			{
				EnumUtil.SetFlag(ref _flags, Flags.AdventureBeganInitialize, value);
			}
		}

		public bool viewMapNodes => _viewMapNodes > 0;

		public int this[ParameterType type]
		{
			get
			{
				return type switch
				{
					ParameterType.StartingAbilityCount => startingAbilityCount, 
					ParameterType.MulliganCount => mulliganCount, 
					ParameterType.JokerCount => jokerCount, 
					ParameterType.RestHealthGain => restHealthGain, 
					ParameterType.RestAbilityDrawCount => restAbilityDrawCount, 
					ParameterType.RestMulliganEnabled => _restMulliganEnabled, 
					ParameterType.LevelUpHealAmount => levelUpHealAmount, 
					ParameterType.InitialLevelUpCount => initialLevelUpAmount, 
					ParameterType.RestMulliganAbilityEnabled => _restMulliganAbilityEnabled, 
					ParameterType.ViewMapNodes => _viewMapNodes, 
					_ => throw new ArgumentOutOfRangeException("type", type, null), 
				};
			}
			set
			{
				switch (type)
				{
				case ParameterType.StartingAbilityCount:
					startingAbilityCount = value;
					break;
				case ParameterType.MulliganCount:
					mulliganCount = value;
					break;
				case ParameterType.JokerCount:
					jokerCount = value;
					break;
				case ParameterType.RestHealthGain:
					restHealthGain = value;
					break;
				case ParameterType.RestAbilityDrawCount:
					restAbilityDrawCount = value;
					break;
				case ParameterType.RestMulliganEnabled:
					_restMulliganEnabled = value;
					break;
				case ParameterType.LevelUpHealAmount:
					levelUpHealAmount = value;
					break;
				case ParameterType.InitialLevelUpCount:
					initialLevelUpAmount = value;
					break;
				case ParameterType.RestMulliganAbilityEnabled:
					_restMulliganAbilityEnabled = value;
					break;
				case ParameterType.ViewMapNodes:
					_viewMapNodes = value;
					break;
				default:
					throw new ArgumentOutOfRangeException("type", type, null);
				}
			}
		}
	}

	[ProtoContract(EnumPassthru = true)]
	public enum ParameterType
	{
		StartingAbilityCount,
		MulliganCount,
		JokerCount,
		RestHealthGain,
		RestAbilityDrawCount,
		RestMulliganEnabled,
		LevelUpHealAmount,
		InitialLevelUpCount,
		RestMulliganAbilityEnabled,
		ViewMapNodes
	}

	[Flags]
	[ProtoContract(EnumPassthru = true)]
	public enum Flags
	{
		AdventureStarted = 1,
		AdventureEnded = 2,
		AdventureBeganInitialize = 4
	}

	public struct RandomOverride : IDisposable
	{
		private readonly GameState _state;

		private readonly System.Random _originalRandom;

		public RandomOverride(GameState state, int seedOffset)
		{
			_state = state;
			_originalRandom = state._random;
			int? dailyLeaderboard = state.dailyLeaderboard;
			if (dailyLeaderboard.HasValue)
			{
				int valueOrDefault = dailyLeaderboard.GetValueOrDefault();
				_state._random = new System.Random(valueOrDefault + state.adventure.data.dailySeedOffset + seedOffset);
			}
		}

		public void Dispose()
		{
			_state._random = _originalRandom;
		}
	}

	private const uint VERSION = 0u;

	public const DamageSources ALL_DAMAGE_SOURCES = DamageSources.Attack | DamageSources.Defense | DamageSources.Ability;

	[ProtoMember(100)]
	private uint _version;

	[ProtoMember(1)]
	private DataRef<GameData> _game;

	[ProtoMember(2)]
	private DataRef<AdventureData> _adventure;

	[ProtoMember(3)]
	private int _registerId;

	[ProtoMember(4)]
	private int _encounterNumber;

	[ProtoMember(5)]
	private int _roundNumber;

	[ProtoMember(6)]
	private Ids<ATarget> _targets;

	[ProtoMember(7)]
	private Id<Player> _player;

	[ProtoMember(8)]
	private Id<AEntity> _entityTakingTurn;

	[ProtoMember(9, OverwriteList = true)]
	private List<AppliedAction> _appliedActions;

	[ProtoMember(10)]
	private IdDeck<ResourceCard.Pile, ResourceCard> _playerResourceDeck;

	[ProtoMember(11)]
	private IdDeck<Ability.Pile, Ability> _abilityDeck;

	[ProtoMember(12)]
	private IdDeck<HeroDeckPile, Ability> _heroDeck;

	[ProtoMember(13)]
	private IdDeck<ResourceCard.Pile, ResourceCard> _enemyResourceDeck;

	[ProtoMember(14)]
	private IdDeck<AdventureCard.Pile, ATarget> _adventureDeck;

	[ProtoMember(15)]
	private IdDeck<ButtonCard.Pile, ButtonCard> _buttonDeck;

	[ProtoMember(16)]
	private IdDeck<TurnOrderSpace.Pile, TurnOrderSpace> _turnOrderSpaceDeck;

	[ProtoMember(17)]
	private IdDeck<Chip.Pile, Chip> _chipDeck;

	[ProtoMember(18)]
	private IdDeck<TutorialCard.Pile, TutorialCard> _tutorialDeck;

	[ProtoMember(19)]
	private IdDeck<Stone.Pile, Stone> _stoneDeck;

	[ProtoMember(20)]
	private IdDeck<ExilePile, ATarget> _exileDeck;

	[ProtoMember(21)]
	private IdDeck<DeckPile, ADeck> _decks;

	[ProtoMember(22)]
	private DataRef<AbilityDeckData> _selectedAbilityDeck;

	[ProtoMember(23)]
	private int _experience;

	[ProtoMember(24)]
	private float _strategyTime;

	[ProtoMember(25)]
	private float _totalTime;

	[ProtoMember(26)]
	private Parameters _parameters;

	[ProtoMember(27)]
	private IdDeck<RewardPile, ATarget> _rewardDeck;

	[ProtoMember(28)]
	private IdDeck<GameStone.Pile, GameStone> _gameStoneDeck;

	[ProtoMember(29, OverwriteList = true)]
	private Dictionary<Id<Ability>, Id<Ability>> _addedTraitMap;

	[ProtoMember(30)]
	private IdDeck<ProceduralMap.Pile, ProceduralMap> _mapDeck;

	[ProtoMember(31)]
	private ProceduralPhaseType? _proceduralPhase;

	[ProtoMember(32)]
	private AdventureCard.SelectInstruction.PlayMusic _lastMusicInstruction;

	[ProtoMember(33)]
	private AdventureCard.SelectInstruction.PlayAmbient _lastAmbientInstruction;

	[ProtoMember(34)]
	private AdventureCard.SelectInstruction.SetLighting _lastLightingInstruction;

	[ProtoMember(35, OverwriteList = true)]
	private List<AdventureCard.SelectInstruction> _loadInstructions;

	[ProtoMember(36)]
	private IdDeck<MapCompass.Pile, MapCompass> _compassDeck;

	[ProtoMember(37)]
	private System.Random _random;

	[ProtoMember(38)]
	private TraitRuleset _traitRuleset;

	[ProtoMember(39)]
	private int? _dailyLeaderboard;

	[ProtoMember(40)]
	private bool _dailyLeaderboardIsValid;

	[ProtoMember(41)]
	private DataRef<ProceduralNodeData> _modifierNode;

	[ProtoMember(42)]
	private int _reloadCount;

	[ProtoMember(43)]
	private float _encounterStartStrategicTime;

	[ProtoMember(44, OverwriteList = true)]
	private List<byte[]> _encounterStrategicTimes;

	[ProtoMember(45, OverwriteList = true)]
	private byte[] _key;

	[ProtoMember(46, OverwriteList = true)]
	private byte[] _iv;

	[ProtoMember(47)]
	private bool _devCommandUsed;

	private GameStepStack _stack;

	private ActiveCombat _activeCombat;

	private bool _roundHasEnded;

	private System.Random _cosmeticRandom;

	private DeckCreationState _deckCreation;

	private LevelUpState _levelUp;

	private ProceduralGraph _graph;

	private HashSet<DataRef<AbilityData>> _unlockedAbilities;

	private int _suppressTraitRemove;

	public static GameState Instance { get; private set; }

	public static NewGameType NewGame => Instance?.game?.data?.newGameType ?? ProfileManager.prefs.selectedGame?.data.newGameType ?? NewGameType.Spring;

	public Ids<ATarget> targets => _targets ?? (_targets = new Ids<ATarget>());

	public List<AppliedAction> appliedActions => _appliedActions ?? (_appliedActions = new List<AppliedAction>());

	public GameStepStack stack => _stack ?? (_stack = new GameStepStack().Register());

	public System.Random random => _random ?? (_random = new System.Random());

	public System.Random cosmeticRandom => _cosmeticRandom ?? (_cosmeticRandom = new System.Random());

	public Player player
	{
		get
		{
			return _player;
		}
		set
		{
			_player = value;
		}
	}

	public IEnumerable<AEntity> turnOrderQueue
	{
		get
		{
			foreach (ATarget card in adventureDeck.GetCards(AdventureCard.Pile.TurnOrder))
			{
				if (card is AEntity aEntity)
				{
					yield return aEntity;
				}
			}
		}
	}

	public Ability activeSummon
	{
		get
		{
			foreach (ATarget card in adventureDeck.GetCards(AdventureCard.Pile.TurnOrder))
			{
				if (card is Ability ability && ability.isSummon)
				{
					return ability;
				}
			}
			return null;
		}
	}

	public IdDeck<ResourceCard.Pile, ResourceCard> playerResourceDeck => _playerResourceDeck ?? (_playerResourceDeck = new IdDeck<ResourceCard.Pile, ResourceCard>());

	public IdDeck<Ability.Pile, Ability> abilityDeck => _abilityDeck ?? (_abilityDeck = new IdDeck<Ability.Pile, Ability>());

	public IdDeck<HeroDeckPile, Ability> heroDeck => _heroDeck ?? (_heroDeck = new IdDeck<HeroDeckPile, Ability>());

	public IdDeck<ResourceCard.Pile, ResourceCard> enemyResourceDeck => _enemyResourceDeck ?? (_enemyResourceDeck = new IdDeck<ResourceCard.Pile, ResourceCard>());

	public IdDeck<AdventureCard.Pile, ATarget> adventureDeck => _adventureDeck ?? (_adventureDeck = new IdDeck<AdventureCard.Pile, ATarget>());

	public IdDeck<ButtonCard.Pile, ButtonCard> buttonDeck => _buttonDeck ?? (_buttonDeck = new IdDeck<ButtonCard.Pile, ButtonCard>());

	public IdDeck<Chip.Pile, Chip> chipDeck => _chipDeck ?? (_chipDeck = new IdDeck<Chip.Pile, Chip>());

	public IdDeck<Stone.Pile, Stone> stoneDeck => _stoneDeck ?? (_stoneDeck = new IdDeck<Stone.Pile, Stone>());

	public IdDeck<TutorialCard.Pile, TutorialCard> tutorialDeck => _tutorialDeck ?? (_tutorialDeck = new IdDeck<TutorialCard.Pile, TutorialCard>());

	public IdDeck<TurnOrderSpace.Pile, TurnOrderSpace> turnOrderSpaceDeck => _turnOrderSpaceDeck ?? (_turnOrderSpaceDeck = new IdDeck<TurnOrderSpace.Pile, TurnOrderSpace>());

	public IdDeck<ExilePile, ATarget> exileDeck => _exileDeck ?? (_exileDeck = new IdDeck<ExilePile, ATarget>());

	public IdDeck<DeckPile, ADeck> decks => _decks ?? (_decks = new IdDeck<DeckPile, ADeck>());

	public IdDeck<RewardPile, ATarget> rewardDeck => _rewardDeck ?? (_rewardDeck = new IdDeck<RewardPile, ATarget>());

	public IdDeck<GameStone.Pile, GameStone> gameStoneDeck => _gameStoneDeck ?? (_gameStoneDeck = new IdDeck<GameStone.Pile, GameStone>());

	public IdDeck<ProceduralMap.Pile, ProceduralMap> mapDeck => _mapDeck ?? (_mapDeck = new IdDeck<ProceduralMap.Pile, ProceduralMap>());

	public ProceduralGraph graph
	{
		get
		{
			return _graph ?? (_graph = mapDeck.GetCards().FirstOrDefault()?.graph);
		}
		set
		{
			_graph = value;
		}
	}

	public IdDeck<MapCompass.Pile, MapCompass> compassDeck => _compassDeck ?? (_compassDeck = new IdDeck<MapCompass.Pile, MapCompass>());

	public ProceduralPhaseType? proceduralPhase
	{
		get
		{
			return _proceduralPhase;
		}
		set
		{
			_proceduralPhase = value;
		}
	}

	public GameStateView view => GameStateView.Instance;

	public ActiveCombat activeCombat
	{
		get
		{
			return _activeCombat;
		}
		set
		{
			if (SetPropertyUtility.SetObject(ref _activeCombat, value))
			{
				this.onCombatIsActiveChanged?.Invoke(_activeCombat != null);
			}
		}
	}

	public bool encounterActive => roundNumber > 0;

	public AEntity entityTakingTurn
	{
		get
		{
			return _entityTakingTurn;
		}
		set
		{
			_entityTakingTurn = value;
		}
	}

	public int encounterNumber
	{
		get
		{
			return _encounterNumber;
		}
		set
		{
			_encounterNumber = value;
		}
	}

	public int roundNumber
	{
		get
		{
			return _roundNumber;
		}
		set
		{
			_roundNumber = value;
		}
	}

	public DataRef<GameData> game
	{
		get
		{
			return _game;
		}
		set
		{
			_game = value;
		}
	}

	public DataRef<AdventureData> adventure
	{
		get
		{
			return _adventure;
		}
		set
		{
			_adventure = value;
		}
	}

	public bool adventureActive
	{
		get
		{
			if (parameters.adventureStarted)
			{
				return !parameters.adventureEnded;
			}
			return false;
		}
	}

	public DataRef<AbilityDeckData> selectedAbilityDeck
	{
		get
		{
			return _selectedAbilityDeck;
		}
		set
		{
			_selectedAbilityDeck = value;
		}
	}

	public int experience
	{
		get
		{
			return _experience;
		}
		set
		{
			_experience = value;
		}
	}

	public int strategyTime => (int)_strategyTime;

	public int totalTime => (int)_totalTime;

	public Parameters parameters => _parameters ?? (_parameters = new Parameters());

	public DeckCreationState deckCreation => _deckCreation ?? (_deckCreation = new DeckCreationState());

	public LevelUpState levelUp => _levelUp ?? (_levelUp = new LevelUpState());

	public bool combatShouldBeCanceled => activeCombat?.shouldBeCanceled ?? true;

	public Dictionary<Id<Ability>, Id<Ability>> addedTraitMap => _addedTraitMap ?? (_addedTraitMap = new Dictionary<Id<Ability>, Id<Ability>>());

	public AdventureCard.SelectInstruction.PlayMusic lastMusicInstruction
	{
		get
		{
			return _lastMusicInstruction;
		}
		set
		{
			_lastMusicInstruction = value;
		}
	}

	public AdventureCard.SelectInstruction.PlayAmbient lastAmbientInstruction
	{
		get
		{
			return _lastAmbientInstruction;
		}
		set
		{
			_lastAmbientInstruction = value;
		}
	}

	public AdventureCard.SelectInstruction.SetLighting lastLightingInstruction
	{
		get
		{
			return _lastLightingInstruction;
		}
		set
		{
			_lastLightingInstruction = value;
		}
	}

	public List<AdventureCard.SelectInstruction> loadInstructions => _loadInstructions ?? (_loadInstructions = new List<AdventureCard.SelectInstruction>());

	public bool wasSerializedWithCurrentVersion => _version == 0;

	public TraitRuleset traitRuleset
	{
		get
		{
			return _traitRuleset;
		}
		set
		{
			_traitRuleset = value;
		}
	}

	public int suppressProcessDamageReactions { get; set; }

	public bool reactToProcessDamage => suppressProcessDamageReactions == 0;

	public int suppressProcessHealReactions { get; set; }

	public bool reactToProcessHeal => suppressProcessHealReactions == 0;

	public bool reactToTraitRemove => _suppressTraitRemove == 0;

	public HashSet<DataRef<AbilityData>> unlockedAbilities => _unlockedAbilities ?? (_unlockedAbilities = new HashSet<DataRef<AbilityData>>(from a in AbilityData.GetAbilities()
		where a.data.characterClass == player.characterClass && (!a.data.upgradeOf || ProfileManager.progress.abilities.read.IsUnlocked(a))
		select a));

	public int? dailyLeaderboard
	{
		get
		{
			return _dailyLeaderboard;
		}
		private set
		{
			_dailyLeaderboard = value;
		}
	}

	public bool dailyLeaderboardIsValid => _dailyLeaderboardIsValid;

	public DataRef<ProceduralNodeData> modifierNode
	{
		get
		{
			return _modifierNode;
		}
		set
		{
			_modifierNode = value;
		}
	}

	public float unlockAbilityForCurrentClassChance => adventure.data.unlockAbilityForCurrentClassChance ?? game.data.unlockAbilityForCurrentClassChance;

	public float keepPreferredAbilityChance => adventure.data.keepPreferredAbilityChance ?? game.data.keepPreferredAbilityChance;

	public int reloadCount
	{
		get
		{
			return _reloadCount;
		}
		set
		{
			_reloadCount = value;
		}
	}

	private List<byte[]> encounterStrategicTimes => _encounterStrategicTimes ?? (_encounterStrategicTimes = new List<byte[]>());

	public bool saving { get; set; }

	public byte[] key => _key ?? (_key = Aes.Create().Key);

	public byte[] iv => _iv ?? (_iv = Aes.Create().IV);

	public bool devCommandUsed
	{
		get
		{
			return _devCommandUsed;
		}
		set
		{
			_devCommandUsed = value;
		}
	}

	private bool _playerSpecified => _player.shouldSerialize;

	private bool _entityTakingTurnSpecified => _entityTakingTurn.shouldSerialize;

	private bool _gameSpecified => _game.ShouldSerialize();

	private bool _adventureSpecified => _adventure.ShouldSerialize();

	private bool _selectedAbilityDeckSpecified => _selectedAbilityDeck.ShouldSerialize();

	private bool _modifierNodeSpecified => _modifierNode.ShouldSerialize();

	public event Action<int> onEncounterStart;

	public event Action<int> onEncounterEnd;

	public event Action<int> onRoundStart;

	public event Action<int> onRoundEnd;

	public event Action<AEntity> onTurnStartEarly;

	public event Action<AEntity> onTurnStart;

	public event Action<AEntity> onTurnStartLate;

	public event Action<AEntity> onTurnEnd;

	public event Action<AEntity, bool, AEntity> onEntityTap;

	public event Action<AEntity, ControlGainType> onControlGained;

	public event OnProcessEnemyCombatStat onProcessEnemyCombatStat;

	public event Action<bool> onCombatIsActiveChanged;

	public event Action<ActiveCombat> onAttackLaunched;

	public event Action<ActiveCombat> onDefensePresent;

	public event Action<ActiveCombat> onDefenseLaunched;

	public event Action<AttackResultType?, ActiveCombat> onCombatVictorDecided;

	public event Action<ActiveCombat> onFinalCombatVictorDecided;

	public event Action<ActiveCombat> onAboutToProcessCombatDamageEarly;

	public event Action<ActiveCombat> onAboutToProcessCombatDamage;

	public event Action<ActiveCombat> onProcessCombatDamage;

	public event Action<ActiveCombat> onCombatEnd;

	public event OnProcessDefenseRules onProcessDefenseRules;

	public event OnProcessAbilityDamage onProcessAbilityDamage;

	public event OnProcessHealAmount onProcessHealAmount;

	public event Action<Ability, List<ATarget>> onBeginToUseAbility;

	public event Action<ActionContext, AAction, List<ATarget>> onAbilityTargeting;

	public event Action<Ability, List<ATarget>, bool> onAbilityAboutToFinish;

	public event Action<Ability, List<ATarget>, bool> onAbilityUsed;

	public event Action<ActionContext, AAction> onAbilityTick;

	public event Action<ActionContext, AAction, int, DamageSource> onDamageDealt;

	public event Action<ActionContext, AAction, int, DamageSource> onTotalDamageDealt;

	public event OnShouldIgnoreShields onShouldIgnoreShields;

	public event Action<ActionContext, AAction, int, DamageSource> onShieldDamageDealt;

	public event Action<ActionContext, AAction, int, DamageSource> onOverkill;

	public event Action<ActionContext, AAction> onDeathsDoor;

	public event Action<ActionContext, AAction> onBeginDeath;

	public event Action<ActionContext, AAction> onDeath;

	public event Action<ActionContext, AAction, int> onHeal;

	public event Action<ActionContext, AAction, int> onOverHeal;

	public event Action<ActionContext, AAction, int> onShieldGain;

	public event Action<ACombatant, int, int> onHPChange;

	public event Action<ACombatant, int, int> onShieldChange;

	public event Action<ACombatant, StatType, int, int> onStatChange;

	public event Action<ACombatant, Ability> onTraitAdded;

	public event Action<ACombatant, Ability> onTraitBeginRemove;

	public event Action<ACombatant, Ability> onTraitRemoved;

	public event OnBuffPlaced onBuffPlaced;

	public event OnBuffRemoved onBuffRemoved;

	public event OnBuffReplaced onBuffReplaced;

	public event OnSummonPlaced onSummonPlaced;

	public event OnSummonRemoved onSummonRemoved;

	public event OnSummonReplaced onSummonReplaced;

	public event Action<ActionContext, TopDeckResult> onTopDeckFinishedDrawing;

	public event Action<ActionContext, TopDeckResult> onTopDeckComplete;

	public event Action<ResourceCard, ResourceCard.WildContext> onWildValueChanged;

	public event Action<ResourceCard> onWildsChanged;

	public event Action<Ability> onAbilityAdded;

	public event Action<ACombatant> onCombatantAdded;

	public event Action<GameState, IdDeckBase> onDeckShuffled;

	public static GameState BeginAdventureSelection(DataRef<AdventureData> autoSelectAdventure = null)
	{
		GameState gameState = new GameState();
		gameState._Initialize();
		gameState.InitializeTutorial();
		gameState.InitializeButtons();
		gameState.InitializeStones();
		gameState.stack.Push(new GameStepGroupDynamic(new GameStepSelectAdventure(autoSelectAdventure)));
		return gameState;
	}

	public static GameState BeginDeckCreation()
	{
		GameState gameState = new GameState();
		gameState._Initialize();
		gameState.stack.Push(new GameStepDeckCreationSelectCharacter());
		return gameState;
	}

	public static GameState BeginLevelUp()
	{
		GameState gameState = new GameState();
		gameState._Initialize();
		return gameState;
	}

	public static void LoadStartupResources()
	{
		GameState gameState = new GameState();
		gameState._Initialize();
		gameState.game = DataRef<GameData>.Search().FirstOrDefault();
		gameState.adventure = DataRef<AdventureData>.Search().FirstOrDefault();
		foreach (DataRef<AdventureData> item in DataRef<AdventureData>.Search())
		{
			gameState.decks.Add(new AdventureDeck(item));
		}
		foreach (DataRef<CharacterData> item2 in DataRef<CharacterData>.Search())
		{
			gameState.adventureDeck.Add(new Player(item2));
		}
		foreach (IAdventureCard item3 in ContentRef.Defaults.data.startingGame.data.adventures.First().data.GenerateCards(gameState).OfType<IAdventureCard>().Distinct(IAdventureCardBlueprintEqualityComparer.Default))
		{
			gameState.adventureDeck.Add(item3.adventureCard);
		}
		DataRef<CharacterData> selectedCharacter = ProfileManager.prefs.selectedCharacter;
		if (selectedCharacter != null && (bool)selectedCharacter)
		{
			foreach (ATarget levelUpCard in selectedCharacter.data.GetLevelUpCards(ProfileManager.progress.experience.read.GetLevel(selectedCharacter)))
			{
				gameState.exileDeck.Add(levelUpCard);
			}
		}
		gameState.InitializePlayerResourceDeck();
		gameState.InitializeEnemyResourceDeck();
		Ability ability = gameState.abilityDeck.Add(new Ability(DataRef<AbilityData>.Search().First()));
		GameStateView.Instance.state = gameState;
		ability.abilityCard.ShowTooltips();
		ResourceCard resourceCard = gameState.playerResourceDeck.GetCards().FirstOrDefault();
		if (resourceCard != null)
		{
			WildValueSelection.Create(resourceCard, (resourceCard.view as ResourceCardView)?.wildValueSelectionContainer);
		}
		DataRef<ProjectileMediaData> dataRef = ContentRef.Defaults.media.warmupMedia ?? DataRef<ProjectileMediaData>.Search().First();
		SimpleProjectileExtrema projectileExtrema = new GameObject("ProjectileExtrema").AddComponent<SimpleProjectileExtrema>();
		projectileExtrema.transform.position = new Vector3(-1000f, -1000f, 0f);
		ProjectileMediaView projectileMediaView = ProjectileMediaView.Create(gameState.cosmeticRandom, dataRef.data, projectileExtrema, projectileExtrema);
		projectileMediaView.transform.SetParent(null, worldPositionStays: true);
		TargetLineView.Add(projectileExtrema, Colors.TARGET, projectileExtrema.transform, projectileExtrema.transform);
		TargetLineView.RemoveOwnedBy(projectileExtrema);
		projectileMediaView.onFinish = (Action)Delegate.Combine(projectileMediaView.onFinish, (Action)delegate
		{
			projectileExtrema.gameObject.Destroy();
		});
		ACombatant aCombatant = gameState.adventureDeck.GetCards().OfType<ACombatant>().First();
		VoiceManager.Instance.Play(aCombatant.view.transform, aCombatant.audio.turnStart, interrupt: false, 2f).SetVolume(0f).Interrupt();
		PauseMenu.Prewarm();
		gameState.Destroy();
	}

	public static GameState GetTempState()
	{
		GameState gameState = new GameState();
		gameState._Initialize();
		return gameState;
	}

	public static int DivideDamage(int damage, int denominator)
	{
		if (denominator >= 2)
		{
			return (damage + denominator - 1) / denominator;
		}
		return damage;
	}

	private GameState()
	{
	}

	private void _Initialize()
	{
		Instance = this;
		_targets = new Ids<ATarget>();
	}

	private void _RegisterStateEvents()
	{
		adventureDeck.onTransfer += _OnAdventureDeckTransfer;
		player.resourceDeck.onTransfer += _OnPlayerResourceDeckTransfer;
		onEntityTap += _OnEntityTap;
		onDeath += _OnDeath;
	}

	private void _OnAdventureDeckTransfer(ATarget card, AdventureCard.Pile? oldPile, AdventureCard.Pile? newPile)
	{
		if (card is AEntity register)
		{
			if (newPile == AdventureCard.Pile.TurnOrder)
			{
				register.Register();
			}
			else if (oldPile == AdventureCard.Pile.TurnOrder)
			{
				register.Unregister();
			}
		}
		if (newPile == AdventureCard.Pile.Discard && card is Ability register2)
		{
			register2.Unregister();
		}
		if ((newPile == AdventureCard.Pile.TurnOrder || oldPile == AdventureCard.Pile.TurnOrder) && card is AEntity)
		{
			RefreshTurnOrderSpaces();
		}
		if (oldPile != AdventureCard.Pile.Draw || !newPile.HasValue || newPile == AdventureCard.Pile.Discard || !(card is IAdventureCard adventureCard))
		{
			return;
		}
		foreach (GameStep onDrawnStep in adventureCard.adventureCardCommon.GetOnDrawnSteps(this))
		{
			stack.Append(onDrawnStep);
		}
	}

	private void _OnPlayerResourceDeckTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (newPile == ResourceCard.Pile.DiscardPile)
		{
			card.wildValue = null;
		}
	}

	private void _OnEntityTap(AEntity entity, bool tapped, AEntity tappedBy)
	{
		RefreshTurnOrderSpacesTapped();
	}

	private void _OnDeath(ActionContext context, AAction action)
	{
		Enemy target = context.GetTarget<Enemy>();
		if (target != null)
		{
			experience += target.enemyData.experience;
		}
	}

	private void _RegisterTargets()
	{
		foreach (ATarget item in from t in targets.Values().AsEnumerable()
			where t.shouldRegisterDuringGameStateInitialization
			orderby t.registerDuringGameStateInitializationOrder
			select t)
		{
			item.Register();
		}
	}

	private PoolKeepItemHashSetHandle<DataRef<BonusCardData>> _GetBonusDataRefs()
	{
		using PoolKeepItemHashSetHandle<DataRef<BonusCardData>> poolKeepItemHashSetHandle2 = Pools.UseKeepItemHashSet(adventure.data.excludedBonuses);
		PoolKeepItemHashSetHandle<DataRef<BonusCardData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<DataRef<BonusCardData>>();
		foreach (DataRef<BonusCardData> item in game.data.bonuses.Concat(adventure.data.additionalBonuses))
		{
			if (!poolKeepItemHashSetHandle2.Contains(item) && (!item.data.oneTimeOnly || !ProfileManager.progress.characters.read[this].HasUnlockedBonus(item)))
			{
				poolKeepItemHashSetHandle.Add(item);
			}
		}
		return poolKeepItemHashSetHandle;
	}

	public void InitializeTutorial()
	{
		tutorialDeck.Add(from dRef in DataRef<TutorialData>.Search()
			where (bool)dRef.data && !ProfileManager.progress.tutorials.read.Contains(dRef)
			select new TutorialCard(dRef));
	}

	public void InitializeAdventureDeck()
	{
		adventureDeck.Add(adventure.data.GenerateCards(this).Reverse());
	}

	public void InitializePlayerResourceDeck()
	{
		playerResourceDeck.Add(EnumUtil<PlayingCardType>.Values.Select((PlayingCardType c) => new ResourceCard(c)).Shuffled(random));
	}

	public void InitializeAbilityDeck()
	{
		abilityDeck.Add(selectedAbilityDeck.data.abilities.Select((DataRef<AbilityData> abilityDataRef) => new Ability(abilityDataRef, player)).Shuffled(random));
	}

	public void InitializeHeroDeck()
	{
		traitRuleset = ProfileManager.options.rebirth.GetTraitRuleset(player.characterDataRef) ?? adventure.data.traitRulesetOverride.GetValueOrDefault();
		PoolKeepItemHashSetHandle<DataRef<AbilityData>> lockedAbilities = player.GetLockedLevelUpAbilities();
		try
		{
			using PoolKeepItemDictionaryHandle<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> poolKeepItemDictionaryHandle = AbilityData.GetNonStandardAbilitiesByCategory(player.characterClass);
			foreach (KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> item in poolKeepItemDictionaryHandle.value)
			{
				if (!item.Key.IsTrait())
				{
					item.Value.value.RemoveAll((DataRef<AbilityData> a) => lockedAbilities.Contains(a));
				}
			}
			foreach (KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> item2 in poolKeepItemDictionaryHandle.value.OrderByDescending((KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> a) => a.Key))
			{
				if (item2.Key.IsTrait())
				{
					if (traitRuleset == TraitRuleset.Unrestricted)
					{
						foreach (DataRef<AbilityData> item3 in from d in poolKeepItemDictionaryHandle.value.Where((KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> p) => p.Key.IsTrait()).SelectMany((KeyValuePair<AbilityData.Category, PoolKeepItemListHandle<DataRef<AbilityData>>> p) => p.Value.value)
							orderby d.data.category descending, d.data.rank descending
							select d)
						{
							heroDeck.Add(new Ability(item3, player));
						}
						continue;
					}
					foreach (DataRef<AbilityData> item4 in item2.Value.value.OrderByDescending((DataRef<AbilityData> d) => d.data.rank))
					{
						heroDeck.Add(new Ability(item4, player));
					}
				}
				else
				{
					DataRef<AbilityData> dataRef = item2.Value.value.MaxBy((DataRef<AbilityData> a) => (float)a.data.rank);
					if (dataRef != null)
					{
						heroDeck.Add(new Ability(dataRef, player));
					}
				}
			}
		}
		finally
		{
			if (lockedAbilities != null)
			{
				((IDisposable)lockedAbilities).Dispose();
			}
		}
	}

	public void InitializeEnemyResourceDeck()
	{
		enemyResourceDeck.Add(EnumUtil<PlayingCardType>.Values.Select((PlayingCardType c) => new ResourceCard(c, PlayingCardSkinType.Enemy)).Shuffled(random));
	}

	public void InitializeButtons()
	{
		buttonDeck.Add(EnumUtil<ButtonCardType>.Values.Select((ButtonCardType c) => new ButtonCard(c)));
	}

	public void InitializeTurnOrderSpaces()
	{
		for (int i = 0; i < 10; i++)
		{
			turnOrderSpaceDeck.Add(new TurnOrderSpace());
		}
	}

	public void InitializeChips()
	{
		for (int i = 0; i < 10; i++)
		{
			chipDeck.Add(ChipType.Attack);
		}
	}

	public void InitializeStones()
	{
		stoneDeck.Add(StoneType.Turn);
		stoneDeck.Add(StoneType.Cancel, Stone.Pile.CancelInactive);
	}

	public void InitializeBonuses()
	{
		foreach (DataRef<BonusCardData> item in _GetBonusDataRefs())
		{
			targets.Add(new BonusCard(item));
		}
	}

	public void StartAdventure()
	{
		_RegisterStateEvents();
		_RegisterTargets();
		stack.Push(new GameStepAdventure());
	}

	public bool Register(IRegister register)
	{
		if (register.IsRegistered())
		{
			return false;
		}
		register._Register();
		register.registerId = ++_registerId;
		return true;
	}

	public bool Unregister(IRegister register)
	{
		if (!register.IsRegistered())
		{
			return false;
		}
		register._Unregister();
		register.registerId = 0;
		return true;
	}

	public void RefreshTurnOrderSpaces()
	{
		int num = adventureDeck.Count(AdventureCard.Pile.TurnOrder) + 1;
		int num2 = turnOrderSpaceDeck.Count(TurnOrderSpace.Pile.Active);
		if (num > num2)
		{
			for (int i = num2; i < num; i++)
			{
				turnOrderSpaceDeck.Transfer(turnOrderSpaceDeck.NextInPile(TurnOrderSpace.Pile.Inactive), TurnOrderSpace.Pile.Active);
			}
		}
		else if (num < num2)
		{
			for (int num3 = num2; num3 > num; num3--)
			{
				turnOrderSpaceDeck.Transfer(turnOrderSpaceDeck.NextInPile(TurnOrderSpace.Pile.Active), TurnOrderSpace.Pile.Inactive);
			}
		}
		RefreshTurnOrderSpacesTapped();
	}

	public void RefreshTurnOrderSpacesTapped()
	{
		using PoolKeepItemListHandle<AEntity> poolKeepItemListHandle2 = Pools.UseKeepItemList(turnOrderQueue);
		using PoolKeepItemListHandle<TurnOrderSpace> poolKeepItemListHandle = Pools.UseKeepItemList(turnOrderSpaceDeck.GetCards(TurnOrderSpace.Pile.Active));
		for (int i = 0; i < poolKeepItemListHandle.Count; i++)
		{
			poolKeepItemListHandle[i].tapped.value = (i < poolKeepItemListHandle2.Count && (bool)poolKeepItemListHandle2[i].tapped) || (i > 0 && (bool)poolKeepItemListHandle2[i - 1].tapped);
		}
	}

	public int GetTurnOrder(AEntity entity)
	{
		return adventureDeck.IndexOf(entity);
	}

	public void SetTurnOrder(AEntity entity, int turnOrderIndex)
	{
		adventureDeck.Transfer(entity, AdventureCard.Pile.TurnOrder, turnOrderIndex);
	}

	public bool WouldChangeTurnOrder(AEntity entity, int newTurnOrderIndex)
	{
		return GetTurnOrder(entity) != Mathf.Clamp(newTurnOrderIndex, 0, adventureDeck.Count(AdventureCard.Pile.TurnOrder) - 1);
	}

	public IEnumerable<AEntity> GetAdjacentEntities(AEntity entity)
	{
		int turnOrder = GetTurnOrder(entity);
		if (turnOrder > 0)
		{
			yield return adventureDeck[AdventureCard.Pile.TurnOrder, turnOrder - 1] as AEntity;
		}
		if (turnOrder < adventureDeck.Count(AdventureCard.Pile.TurnOrder) - 1)
		{
			yield return adventureDeck[AdventureCard.Pile.TurnOrder, turnOrder + 1] as AEntity;
		}
	}

	public IEnumerable<AEntity> GetEntities(Faction faction)
	{
		foreach (AEntity item in turnOrderQueue)
		{
			if (item.faction == faction)
			{
				yield return item;
			}
		}
	}

	public IEnumerable<AEntity> GetEntities(AEntity entity, Allegiance? allegiance)
	{
		if (!allegiance.HasValue)
		{
			return turnOrderQueue;
		}
		return GetEntities((allegiance == Allegiance.Foe) ? entity.faction.Opponent() : entity.faction);
	}

	public IEnumerable<AEntity> GetEnemies(AEntity entity)
	{
		return GetEntities(entity.faction.Opponent());
	}

	public IEnumerable<Ability> GetAppliedAbilities()
	{
		foreach (AEntity item in turnOrderQueue)
		{
			if (!(item is ACombatant aCombatant))
			{
				continue;
			}
			foreach (Ability card in aCombatant.appliedAbilities.GetCards())
			{
				yield return card;
			}
		}
	}

	public IEnumerable<Ability> GetAbilitiesInTurnOrder()
	{
		foreach (AEntity item in turnOrderQueue)
		{
			if (item is ACombatant aCombatant)
			{
				foreach (Ability card in aCombatant.appliedAbilities.GetCards())
				{
					yield return card;
				}
			}
			else if (item is Ability ability)
			{
				yield return ability;
			}
		}
	}

	public void CheckForEnemiesThatShouldBeDead()
	{
		foreach (AEntity item in Pools.UseKeepItemList(turnOrderQueue))
		{
			if (item is Enemy enemy && (int)enemy.HP <= 0)
			{
				stack.Append(new GameStepDeath(new ActionContext(player, null, enemy), null));
			}
		}
	}

	public AEntity GetNextEntityInTurnOrder()
	{
		foreach (AEntity item in turnOrderQueue)
		{
			if (!item.hasTakenTurn)
			{
				return item;
			}
		}
		return null;
	}

	public bool SignalEndTurn()
	{
		if (entityTakingTurn == null)
		{
			return false;
		}
		entityTakingTurn.SetLeftOfEntitiesHasTakenTurn();
		entityTakingTurn.view.offsets.Remove(AGameStepTurn.OFFSET);
		entityTakingTurn.OnTurnEnd();
		entityTakingTurn = null;
		return true;
	}

	public bool SignalEndRound()
	{
		if (_roundHasEnded)
		{
			return false;
		}
		if (GetNextEntityInTurnOrder() != null && GetEncounterState() == EncounterState.Active)
		{
			return false;
		}
		this.onRoundEnd?.Invoke(roundNumber);
		return _roundHasEnded = true;
	}

	public void GoToNextRound(bool incrementRound = true)
	{
		_roundHasEnded = false;
		if (incrementRound)
		{
			int num = roundNumber + 1;
			roundNumber = num;
		}
		foreach (AEntity item in turnOrderQueue)
		{
			item.OnRoundStart();
		}
		this.onRoundStart?.Invoke(roundNumber);
	}

	public EncounterState? GetEncounterState()
	{
		if (_roundNumber <= 0)
		{
			return null;
		}
		if ((bool)player.dead)
		{
			return EncounterState.Failure;
		}
		foreach (AEntity item in turnOrderQueue)
		{
			if (item.faction == Faction.Enemy)
			{
				return EncounterState.Active;
			}
		}
		return EncounterState.Victory;
	}

	public void DealDamage(ActionContext context, int damage, DamageSource source, AAction action = null, bool? ignoreShields = null)
	{
		stack.Append(new GameStepDealDamage(context, damage, source, action, ignoreShields));
	}

	public void Heal(ActionContext context, int heal, AAction action = null, bool allowHealingOverMaxHP = false, AEntity actorOverride = null)
	{
		if (heal < 0)
		{
			if (actorOverride != null)
			{
				context = context.SetActor(actorOverride);
			}
			DamageAction.Damage(action, context, -heal);
			return;
		}
		ACombatant target = context.GetTarget<ACombatant>();
		int num = (allowHealingOverMaxHP ? heal : Math.Min(target.HPMissing, heal));
		int num2 = heal - num;
		target.HP.value += num;
		if (num > 0)
		{
			this.onHeal?.Invoke(context, action, num);
		}
		if (num2 > 0)
		{
			this.onOverHeal?.Invoke(context, action, num2);
		}
	}

	public void ClearAppliedActionsOn<T>(ATarget appliedOn, Func<T, bool> validAction = null) where T : AAction
	{
		for (int num = appliedActions.Count - 1; num >= 0; num--)
		{
			if (appliedActions[num].context.target == appliedOn && appliedActions[num].action is T arg && (validAction == null || validAction(arg)))
			{
				appliedActions[num].Unapply();
			}
		}
	}

	public void AdjustTotalTime(float deltaTime)
	{
		_totalTime += deltaTime;
	}

	public void AdjustStrategyTime(float deltaTime)
	{
		_strategyTime += deltaTime;
	}

	public bool EntityWithStatusExists(StatusType status, Faction faction)
	{
		foreach (ATarget item in adventureDeck.GetCardsSafe(AdventureCard.Pile.TurnOrder))
		{
			if (item is AEntity aEntity && aEntity.faction == faction && aEntity.HasStatus(status))
			{
				return true;
			}
		}
		return false;
	}

	public bool EntityWithoutStatusExists(StatusType status, Faction faction)
	{
		foreach (ATarget item in adventureDeck.GetCardsSafe(AdventureCard.Pile.TurnOrder))
		{
			if (item is AEntity aEntity && aEntity.faction == faction && !aEntity.HasStatus(status))
			{
				return true;
			}
		}
		return false;
	}

	public void KillCombatant(ACombatant combatant)
	{
		_suppressTraitRemove++;
		combatant.RemoveTraits();
		_suppressTraitRemove--;
		KillAction.Kill(new ActionContext(player, null, combatant), combatant);
	}

	public void KillAllEnemies()
	{
		foreach (ACombatant item in GetEntities(Faction.Enemy).OfType<ACombatant>())
		{
			KillCombatant(item);
		}
	}

	public void ProcessProceduralNodeData(ProceduralNodeData proceduralNodeData, bool useRewardLayouts = false)
	{
		if (useRewardLayouts)
		{
			view.adventureDeckLayout.SetLayout(AdventureCard.Pile.Draw, view.rewardDeckLayout.draw);
			view.adventureDeckLayout.SetLayout(AdventureCard.Pile.Discard, view.rewardDeckLayout.discard);
		}
		proceduralNodeData.GenerateCards(this);
		if (!adventureActive)
		{
			stack.Append(new GameStepAdventureNarrationLoop());
		}
		foreach (GameStep step in proceduralNodeData.onSelectInstructions.GetSteps(this))
		{
			stack.Append(step);
		}
		if (useRewardLayouts)
		{
			stack.Append(new GameStepGenericSimple(delegate
			{
				view.adventureDeckLayout.RestoreLayoutToDefaultWithoutTransferringCards(AdventureCard.Pile.Draw);
				view.adventureDeckLayout.RestoreLayoutToDefaultWithoutTransferringCards(AdventureCard.Pile.Discard);
			}));
		}
	}

	public void PushOnLoadGameSteps()
	{
		ProfileManager.options.cosmetic.SignalPlayingCardDeckChange();
		stack.Push(new GameStepRestoreGameState());
		if (lastMusicInstruction != null)
		{
			foreach (GameStep gameStep in lastMusicInstruction.GetGameSteps(this))
			{
				stack.Push(gameStep);
			}
		}
		if (lastAmbientInstruction != null)
		{
			foreach (GameStep gameStep2 in lastAmbientInstruction.GetGameSteps(this))
			{
				stack.Push(gameStep2);
			}
		}
		if (lastLightingInstruction != null)
		{
			foreach (GameStep gameStep3 in lastLightingInstruction.GetGameSteps(this))
			{
				stack.Push(gameStep3);
			}
		}
		foreach (GameStep step in loadInstructions.GetSteps(this))
		{
			stack.Push(step);
		}
		loadInstructions.Clear();
		if (encounterActive)
		{
			view.OffsetTurnOrderForCombat(offset: true);
			stack.Push(new GameStepGenericSimple(delegate
			{
				GameStepEncounter.SetEntityTakingTurnOffsets(entityTakingTurn);
			}));
			stack.Push(entityTakingTurn.GetTurnStep());
			stack.Push(new GameStepEncounter());
		}
		if (mapDeck.Any())
		{
			stack.Push(new GameStepProceduralMap());
		}
		stack.Push(new GameStepAdventure());
	}

	public void Destroy()
	{
		stack.Cancel();
		UnregisterEvents.Unregister(this);
		UPools.Clear(updateBeforeClearing: true);
		GameStateView.DestroyInstance();
		Instance = null;
	}

	public void CalculateDailySeed(int? dayOverride = null)
	{
		int? num = dailyLeaderboard;
		int? day = LeaderboardProgress.GetDay();
		_dailyLeaderboardIsValid = day.HasValue;
		if (dayOverride.HasValue)
		{
			dailyLeaderboard = dayOverride.Value;
		}
		int? num2 = dailyLeaderboard ?? ProfileManager.options.randomizedDailySeed ?? day ?? LeaderboardProgress.GetDay(DateTime.UtcNow.GetUnixEpoch());
		if (num2.HasValue)
		{
			int valueOrDefault = num2.GetValueOrDefault();
			SetRandomSeed((dailyLeaderboard = valueOrDefault) + adventure.data.dailySeedOffset);
		}
		if (dayOverride.HasValue)
		{
			dailyLeaderboard = num;
		}
	}

	public IEnumerable<ATarget> GetModifierCards(DataRef<AdventureData> adventureRef, DataRef<ProceduralNodePackData> modifierRef, int? dayOverride = null, DataRef<ProceduralNodeData> modifierOverride = null)
	{
		DataRef<AdventureData> tempAdventure = adventure;
		adventure = adventureRef;
		CalculateDailySeed(dayOverride);
		DataRef<ProceduralNodeData> node = modifierRef.data.pack.GetSelection(random).node;
		if (node != null && (bool)node)
		{
			foreach (ATarget item in (modifierOverride ?? node).data.cards.GenerateCards(this))
			{
				yield return item;
			}
		}
		adventure = tempAdventure;
	}

	public void ClearDailySeed()
	{
		_random = null;
	}

	private void SetRandomSeed(int? seed)
	{
		if (seed.HasValue)
		{
			_random = new System.Random(seed.Value);
		}
	}

	public RandomOverride OverrideRandom(int seedOffset)
	{
		return new RandomOverride(this, seedOffset);
	}

	public void EncounterStartStrategicTime()
	{
		if (dailyLeaderboardIsValid)
		{
			_encounterStartStrategicTime = _strategyTime;
		}
	}

	public void EncounterEndStrategicTime()
	{
		if (dailyLeaderboardIsValid)
		{
			encounterStrategicTimes.Add(ProtoUtil.Encrypt(_strategyTime - _encounterStartStrategicTime, key, iv));
		}
	}

	public bool LeaderboardIsValid()
	{
		if (dailyLeaderboardIsValid && encounterStrategicTimes.Count == _encounterNumber && Math.Abs(encounterStrategicTimes.Select((byte[] b) => ProtoUtil.Decrypt<float>(b, key, iv)).Sum() - _strategyTime) <= 1f)
		{
			return experience <= 9999;
		}
		return false;
	}

	public int LeaderboardManaMax()
	{
		return adventureDeck.GetCards(AdventureCard.Pile.Discard).OfType<Enemy>().Sum((Enemy e) => e.enemyData.experience) + rewardDeck.GetCards(RewardPile.Results).OfType<BonusCard>().Sum((BonusCard b) => b.experience);
	}

	public void SignalShouldIgnoreShields(ActionContext context, AAction action, int damage, DamageSource source, ref bool? ignoreShields)
	{
		this.onShouldIgnoreShields?.Invoke(context, action, damage, source, ref ignoreShields);
	}

	public void SignalShieldDamageDealt(ActionContext context, AAction action, int damagedShield, DamageSource source)
	{
		this.onShieldDamageDealt?.Invoke(context, action, damagedShield, source);
	}

	public void SignalDamageDealt(ActionContext context, AAction action, int damage, DamageSource source)
	{
		this.onDamageDealt?.Invoke(context, action, damage, source);
	}

	public void SignalTotalDamageDealt(ActionContext context, AAction action, int totalDamage, DamageSource source)
	{
		this.onTotalDamageDealt?.Invoke(context, action, totalDamage, source);
	}

	public void SignalOverkill(ActionContext context, AAction action, int overkill, DamageSource source)
	{
		this.onOverkill?.Invoke(context, action, overkill, source);
	}

	public void SignalEncounterStart()
	{
		this.onEncounterStart?.Invoke(encounterNumber);
	}

	public void SignalEncounterEnd()
	{
		this.onEncounterEnd?.Invoke(encounterNumber);
	}

	public void SignalTurnStartEarly(AEntity entity)
	{
		this.onTurnStartEarly?.Invoke(entity);
	}

	public void SignalTurnStart(AEntity entity)
	{
		this.onTurnStart?.Invoke(entity);
	}

	public void SignalTurnStartLate(AEntity entity)
	{
		this.onTurnStartLate?.Invoke(entity);
	}

	public void SignalTurnEnd(AEntity entity)
	{
		this.onTurnEnd?.Invoke(entity);
	}

	public void SignalEntityTap(AEntity entity, bool tapped, AEntity tappedBy = null)
	{
		this.onEntityTap?.Invoke(entity, tapped, tappedBy);
	}

	public void SignalControlGained(AEntity entity, ControlGainType controlGainType)
	{
		this.onControlGained?.Invoke(entity, controlGainType);
	}

	public void SignalCombatantAdded(ACombatant combatant)
	{
		this.onCombatantAdded?.Invoke(combatant);
	}

	public PokerHandTypes ProcessDefenseRules(ACombatant attacker, ACombatant defender, PokerHand attackHand = null)
	{
		if (attackHand == null)
		{
			return EnumUtil<PokerHandTypes>.AllFlags;
		}
		PokerHandTypes defenseHands = attackHand.type.HandsOfSameSize();
		this.onProcessDefenseRules?.Invoke(attacker, defender, attackHand, ref defenseHands);
		return defenseHands;
	}

	public void SignalAttackLaunched()
	{
		activeCombat.attackHasBeenLaunched = true;
		CappedBInt numberOfAttacks = activeCombat.attacker.numberOfAttacks;
		int value = numberOfAttacks.value - 1;
		numberOfAttacks.value = value;
		this.onAttackLaunched?.Invoke(activeCombat);
	}

	public void SignalDefensePresent()
	{
		this.onDefensePresent?.Invoke(activeCombat);
	}

	public void SignalDefenseLaunched()
	{
		activeCombat.defenseHasBeenLaunched = true;
		this.onDefenseLaunched?.Invoke(activeCombat);
	}

	public void SignalCombatVictorDecided(AttackResultType? previousResult)
	{
		this.onCombatVictorDecided?.Invoke(previousResult, activeCombat);
	}

	public void SignalFinalCombatVictorDecided()
	{
		this.onFinalCombatVictorDecided?.Invoke(activeCombat);
	}

	public void SignalAboutToProcessCombatDamageEarly()
	{
		this.onAboutToProcessCombatDamageEarly?.Invoke(activeCombat);
	}

	public void SignalAboutToProcessCombatDamage()
	{
		this.onAboutToProcessCombatDamage?.Invoke(activeCombat);
	}

	public void SignalProcessCombatDamage()
	{
		this.onProcessCombatDamage?.Invoke(activeCombat);
	}

	public void SignalOnCombatEnd()
	{
		this.onCombatEnd?.Invoke(activeCombat);
	}

	public void SignalShieldGain(ActionContext context, AAction action, int shieldAmount)
	{
		this.onShieldGain?.Invoke(context, action, shieldAmount);
	}

	public void SignalHPChange(ACombatant combatant, int oldHP, int newHP)
	{
		this.onHPChange?.Invoke(combatant, oldHP, newHP);
	}

	public void SignalShieldChange(ACombatant combatant, int oldShield, int newShield)
	{
		this.onShieldChange?.Invoke(combatant, oldShield, newShield);
	}

	public void SignalStatChange(ACombatant combatant, StatType stat, int oldStat, int newStat)
	{
		this.onStatChange?.Invoke(combatant, stat, oldStat, newStat);
	}

	public int ProcessAbilityDamage(ActionContext context, AAction action, int damage)
	{
		int damageMultiplier = 1;
		int damageDenominator = 1;
		this.onProcessAbilityDamage?.Invoke(context, action, ref damage, ref damageMultiplier, ref damageDenominator);
		return DivideDamage(damage * damageMultiplier, damageDenominator);
	}

	public int ProcessHealAmount(ActionContext context, AAction action, int heal, out AEntity actorOverride, bool suppressReactions = false)
	{
		int healMultiplier = 1;
		int healDenominator = 1;
		actorOverride = null;
		if (suppressReactions)
		{
			int num = suppressProcessHealReactions + 1;
			suppressProcessHealReactions = num;
		}
		this.onProcessHealAmount?.Invoke(context, action, ref heal, ref healMultiplier, ref healDenominator, ref actorOverride);
		if (suppressReactions)
		{
			int num = suppressProcessHealReactions - 1;
			suppressProcessHealReactions = num;
		}
		return DivideDamage(heal * healMultiplier, healDenominator);
	}

	public int ProcessEnemyCombatStat(ACombatant attacker, ACombatant defender, int effectiveCombatStat, bool shouldTriggerMedia = false, int? minValue = null)
	{
		this.onProcessEnemyCombatStat?.Invoke(attacker, defender, ref effectiveCombatStat, shouldTriggerMedia);
		if (minValue.HasValue)
		{
			effectiveCombatStat = Math.Max(minValue.Value, effectiveCombatStat);
		}
		return effectiveCombatStat;
	}

	public void SignalBeginToUseAbility(Ability ability, List<ATarget> abilityTargets)
	{
		this.onBeginToUseAbility?.Invoke(ability, abilityTargets);
	}

	public void SignalAbilityTargeting(ActionContext context, AAction action, List<ATarget> abilityTargets)
	{
		this.onAbilityTargeting?.Invoke(context, action, abilityTargets);
	}

	public void SignalAbilityAboutToFinish(GameStepAbilityActComplete completionStep)
	{
		_SignalAbilityUsed(completionStep, this.onAbilityAboutToFinish);
	}

	public void SignalAbilityUsed(GameStepAbilityActComplete completionStep)
	{
		_SignalAbilityUsed(completionStep, this.onAbilityUsed);
	}

	private void _SignalAbilityUsed(GameStepAbilityActComplete completionStep, Action<Ability, List<ATarget>, bool> action)
	{
		if (action == null)
		{
			return;
		}
		using PoolKeepItemListHandle<ATarget> poolKeepItemListHandle = Pools.UseKeepItemList<ATarget>();
		foreach (GameStep previousStep in completionStep.GetPreviousSteps(GameStep.GroupType.Context))
		{
			if (!(previousStep is GameStepActionTarget gameStepActionTarget))
			{
				continue;
			}
			foreach (ATarget target in gameStepActionTarget.targets)
			{
				poolKeepItemListHandle.Add(target);
			}
		}
		action(completionStep.ability, poolKeepItemListHandle, completionStep.interrupted);
	}

	public void SignalAbilityAdded(Ability ability)
	{
		this.onAbilityAdded?.Invoke(ability);
	}

	public void SignalAbilityTick(ActionContext context, AAction action)
	{
		this.onAbilityTick?.Invoke(context, action);
	}

	public void SignalEntityOnDeathsDoor(ActionContext context, AAction action)
	{
		this.onDeathsDoor?.Invoke(context, action);
	}

	public void SignalEntityBeginDeath(ActionContext context, AAction action)
	{
		this.onBeginDeath?.Invoke(context, action);
	}

	public void SignalEntityDeath(ActionContext context, AAction action)
	{
		this.onDeath?.Invoke(context, action);
	}

	public void SignalTraitAdded(ACombatant combatant, Ability trait)
	{
		this.onTraitAdded?.Invoke(combatant, trait);
	}

	public void SignalTraitBeginRemove(ACombatant combatant, Ability trait)
	{
		this.onTraitBeginRemove?.Invoke(combatant, trait);
	}

	public void SignalTraitRemoved(ACombatant combatant, Ability trait)
	{
		this.onTraitRemoved?.Invoke(combatant, trait);
	}

	public void SignalBuffPlaced(ACombatant actor, ACombatant effected, Ability buffBeingPlaced)
	{
		this.onBuffPlaced?.Invoke(actor, effected, buffBeingPlaced);
	}

	public void SignalBuffRemoved(ACombatant actor, ACombatant effected, Ability buffBeingRemoved)
	{
		this.onBuffRemoved?.Invoke(actor, effected, buffBeingRemoved);
	}

	public void SignalBuffReplaced(ACombatant actor, ACombatant effected, Ability buffBeingReplaced, Ability buffThatIsReplacing)
	{
		this.onBuffReplaced?.Invoke(actor, effected, buffBeingReplaced, buffThatIsReplacing);
	}

	public void SignalSummonPlaced(Ability summonBeingPlaced)
	{
		this.onSummonPlaced?.Invoke(player, summonBeingPlaced);
	}

	public void SignalSummonRemoved(Ability summonBeingRemoved, bool isFinishedBeingRemoved, bool beingReplaced)
	{
		this.onSummonRemoved?.Invoke(player, summonBeingRemoved, isFinishedBeingRemoved, beingReplaced);
	}

	public void SignalSummonReplaced(Ability summonBeingReplaced, Ability summonThatIsReplacing)
	{
		this.onSummonReplaced?.Invoke(player, summonBeingReplaced, summonThatIsReplacing);
	}

	public void SignalTopDeckFinishedDrawing(ActionContext context, TopDeckResult result)
	{
		this.onTopDeckFinishedDrawing?.Invoke(context, result);
	}

	public void SignalTopDeckComplete(ActionContext context, TopDeckResult result)
	{
		this.onTopDeckComplete?.Invoke(context, result);
	}

	public void SignalWildValueChanged(ResourceCard card, ResourceCard.WildContext context)
	{
		this.onWildValueChanged?.Invoke(card, context);
	}

	public void SignalWildsChanged(ResourceCard card)
	{
		this.onWildsChanged?.Invoke(card);
	}

	public void SignalDeckShuffled(IdDeckBase deck)
	{
		this.onDeckShuffled?.Invoke(this, deck);
	}

	[ProtoBeforeSerialization]
	private void _ProtoBeforeSerialization()
	{
		_version = 0u;
	}

	[ProtoAfterDeserialization]
	private void _ProtoAfterDeserialization()
	{
		Instance = this;
		_RegisterStateEvents();
		using PoolKeepItemListHandle<IRegister> poolKeepItemListHandle = Pools.UseKeepItemList<IRegister>();
		foreach (ATarget item in targets.Values())
		{
			if (item.IsRegistered())
			{
				poolKeepItemListHandle.Add(item);
			}
		}
		foreach (AppliedAction appliedAction in appliedActions)
		{
			if (appliedAction.IsRegistered())
			{
				poolKeepItemListHandle.Add(appliedAction);
			}
		}
		poolKeepItemListHandle.value.Sort(IRegisterComparer.Default);
		foreach (IRegister item2 in poolKeepItemListHandle.value)
		{
			item2._Register();
		}
	}
}
