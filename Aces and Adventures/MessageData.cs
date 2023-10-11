using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine.Localization;

[ProtoContract]
[UIField]
public class MessageData : IDataContent
{
	[ProtoContract]
	[UIField]
	public class AttackMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<CanAttackResult.PreventedBy> _preventedBy;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<NumberOfAttacks> _remaining;

		public EnumToLocalizedStringMap<CanAttackResult.PreventedBy> preventedBy => _preventedBy ?? (_preventedBy = new EnumToLocalizedStringMap<CanAttackResult.PreventedBy>());

		public EnumToLocalizedStringMap<NumberOfAttacks> remaining => _remaining ?? (_remaining = new EnumToLocalizedStringMap<NumberOfAttacks>());
	}

	[ProtoContract]
	[UIField]
	public class AbilityMessages
	{
		[ProtoContract]
		[UIField]
		public class PreventedBy
		{
			[ProtoMember(1)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityPreventedBy> _error;

			[ProtoMember(2)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityPreventedBy> _reaction;

			public EnumToLocalizedStringMap<AbilityPreventedBy> error => _error ?? (_error = new EnumToLocalizedStringMap<AbilityPreventedBy>());

			public EnumToLocalizedStringMap<AbilityPreventedBy> reaction => _reaction ?? (_reaction = new EnumToLocalizedStringMap<AbilityPreventedBy>());
		}

		[ProtoContract]
		[UIField]
		public class Keyword
		{
			[ProtoMember(1)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityKeyword> _tooltip;

			[ProtoMember(2)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityKeyword> _tag;

			[ProtoMember(3)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityKeyword> _searchFilter;

			public EnumToLocalizedStringMap<AbilityKeyword> tooltip => _tooltip ?? (_tooltip = new EnumToLocalizedStringMap<AbilityKeyword>());

			public EnumToLocalizedStringMap<AbilityKeyword> tag => _tag ?? (_tag = new EnumToLocalizedStringMap<AbilityKeyword>());

			public EnumToLocalizedStringMap<AbilityKeyword> searchFilter => _searchFilter ?? (_searchFilter = new EnumToLocalizedStringMap<AbilityKeyword>());
		}

		[ProtoContract]
		[UIField]
		public class Category
		{
			[ProtoMember(1)]
			[UIField]
			private EnumToLocalizedStringMap<AbilityData.Category> _levelUp;

			public EnumToLocalizedStringMap<AbilityData.Category> levelUp => _levelUp ?? (_levelUp = new EnumToLocalizedStringMap<AbilityData.Category>());
		}

		[ProtoContract]
		[UIField]
		public class Cost
		{
			[ProtoMember(1)]
			[UIField]
			private EnumToLocalizedStringMap<ResourceCostIconType> _tooltip;

			public EnumToLocalizedStringMap<ResourceCostIconType> tooltip => _tooltip ?? (_tooltip = new EnumToLocalizedStringMap<ResourceCostIconType>());
		}

		[ProtoMember(1)]
		[UIField]
		private PreventedBy _preventedBy;

		[ProtoMember(2)]
		[UIField]
		private Keyword _keyword;

		[ProtoMember(3)]
		[UIField]
		private Category _category;

		[ProtoMember(4)]
		[UIField]
		private LocalizedStringData.AVariable.LocalizedStringVariable _name;

		[ProtoMember(5)]
		[UIField]
		private Cost _cost;

		public PreventedBy preventedBy => _preventedBy ?? (_preventedBy = new PreventedBy());

		public Keyword keyword => _keyword ?? (_keyword = new Keyword());

		public Category category => _category ?? (_category = new Category());

		public LocalizedStringData.TableEntryId nameTableEntryId => _name.localizedString;

		public Cost cost => _cost ?? (_cost = new Cost());
	}

	[ProtoContract]
	[UIField]
	public class DiscardMessages
	{
		[ProtoContract]
		[UIField]
		public class Count
		{
			[ProtoMember(1)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _ability;

			[ProtoMember(2)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _abilityInstruction;

			[ProtoMember(3)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _resource;

			[ProtoMember(4)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _resourceInstruction;

			[ProtoMember(5)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _mulligan;

			[ProtoMember(6)]
			[UIField]
			private EnumToLocalizedStringMap<DiscardCount> _mulliganInstruction;

			public EnumToLocalizedStringMap<DiscardCount> ability => _ability ?? (_ability = new EnumToLocalizedStringMap<DiscardCount>());

			public EnumToLocalizedStringMap<DiscardCount> abilityInstruction => _abilityInstruction ?? (_abilityInstruction = new EnumToLocalizedStringMap<DiscardCount>());

			public EnumToLocalizedStringMap<DiscardCount> resource => _resource ?? (_resource = new EnumToLocalizedStringMap<DiscardCount>());

			public EnumToLocalizedStringMap<DiscardCount> resourceInstruction => _resourceInstruction ?? (_resourceInstruction = new EnumToLocalizedStringMap<DiscardCount>());

			public EnumToLocalizedStringMap<DiscardCount> mulligan => _mulligan ?? (_mulligan = new EnumToLocalizedStringMap<DiscardCount>());

			public EnumToLocalizedStringMap<DiscardCount> mulliganInstruction => _mulliganInstruction ?? (_mulliganInstruction = new EnumToLocalizedStringMap<DiscardCount>());
		}

		[ProtoMember(1)]
		[UIField]
		private Count _count;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<DiscardReason> _reason;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private LocalizedStringData.AVariable.LocalizedStringVariable _combined;

		public Count count => _count ?? (_count = new Count());

		public EnumToLocalizedStringMap<DiscardReason> reason => _reason ?? (_reason = new EnumToLocalizedStringMap<DiscardReason>());

		public LocalizedString combined => _combined?.localizedString;
	}

	[ProtoContract]
	[UIField]
	public class PickMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<PickCount> _selectStandard;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<PickCount> _selectFlexible;

		[ProtoMember(3)]
		[UIField]
		private EnumToLocalizedStringMap<PickCount> _selectFlexibleInstruction;

		public EnumToLocalizedStringMap<PickCount> select => _selectStandard ?? (_selectStandard = new EnumToLocalizedStringMap<PickCount>());

		public EnumToLocalizedStringMap<PickCount> selectFlexible => _selectFlexible ?? (_selectFlexible = new EnumToLocalizedStringMap<PickCount>());

		public EnumToLocalizedStringMap<PickCount> selectFlexibleInstruction => _selectFlexibleInstruction ?? (_selectFlexibleInstruction = new EnumToLocalizedStringMap<PickCount>());
	}

	[ProtoContract]
	[UIField]
	public class TargetMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<SelectableTargetType> _type;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<SelectableTargetCount> _count;

		[ProtoMember(3)]
		[UIField]
		[UIDeepValueChange]
		private LocalizedStringData.AVariable.LocalizedStringVariable _combined;

		public EnumToLocalizedStringMap<SelectableTargetType> type => _type ?? (_type = new EnumToLocalizedStringMap<SelectableTargetType>());

		public EnumToLocalizedStringMap<SelectableTargetCount> count => _count ?? (_count = new EnumToLocalizedStringMap<SelectableTargetCount>());

		public LocalizedString combined => _combined?.localizedString;
	}

	[ProtoContract]
	[UIField]
	public class TutorialMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<PlayerTurnTutorial> _playerTurn;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<AdventureTutorial> _adventure;

		public EnumToLocalizedStringMap<PlayerTurnTutorial> playerTurn => _playerTurn ?? (_playerTurn = new EnumToLocalizedStringMap<PlayerTurnTutorial>());

		public EnumToLocalizedStringMap<AdventureTutorial> adventure => _adventure ?? (_adventure = new EnumToLocalizedStringMap<AdventureTutorial>());
	}

	[ProtoContract]
	[UIField]
	public class ButtonMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<Stone.Pile> _stoneTooltip;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<ButtonCardType> _buttonTooltip;

		public EnumToLocalizedStringMap<Stone.Pile> stoneTooltip => _stoneTooltip ?? (_stoneTooltip = new EnumToLocalizedStringMap<Stone.Pile>());

		public EnumToLocalizedStringMap<ButtonCardType> buttonTooltip => _buttonTooltip ?? (_buttonTooltip = new EnumToLocalizedStringMap<ButtonCardType>());
	}

	[ProtoContract]
	[UIField]
	public class PlayerMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<PlayerClass> _playerClass;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<PlayerClass> _playerClassUnlock;

		[ProtoMember(3)]
		[UIField]
		private EnumToLocalizedStringMap<EndTurnPreventedBy> _endTurnPreventedBy;

		public EnumToLocalizedStringMap<PlayerClass> playerClass => _playerClass ?? (_playerClass = new EnumToLocalizedStringMap<PlayerClass>());

		public EnumToLocalizedStringMap<PlayerClass> playerClassUnlock => _playerClassUnlock ?? (_playerClassUnlock = new EnumToLocalizedStringMap<PlayerClass>());

		public EnumToLocalizedStringMap<EndTurnPreventedBy> endTurnPreventedBy => _endTurnPreventedBy ?? (_endTurnPreventedBy = new EnumToLocalizedStringMap<EndTurnPreventedBy>());
	}

	[ProtoContract]
	[UIField]
	public class GameMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<AdventureResultType> _adventureResult;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<NewGameType> _newGame;

		[ProtoMember(3)]
		[UIField]
		private EnumToLocalizedStringMap<LevelUpMessages> _levelUp;

		[ProtoMember(4)]
		[UIField]
		private EnumToLocalizedStringMap<LeafLevel> _leaf;

		[ProtoMember(5)]
		[UIField]
		private EnumToLocalizedStringMap<AdventureCompletionRank> _adventureCompletionRank;

		[ProtoMember(6)]
		[UIField]
		private EnumToLocalizedStringMap<GameTooltips> _tooltips;

		[ProtoMember(7)]
		[UIField]
		[UIDeepValueChange]
		private LocalizedStringData.AVariable.LocalizedStringVariable _levelUpCombined;

		[ProtoMember(8)]
		[UIField]
		private EnumToLocalizedStringMap<ProceduralNodeType> _proceduralNodeNames;

		[ProtoMember(9)]
		[UIField]
		private EnumToLocalizedStringMap<RebirthLevel> _rebirth;

		public EnumToLocalizedStringMap<AdventureResultType> adventureResult => _adventureResult ?? (_adventureResult = new EnumToLocalizedStringMap<AdventureResultType>());

		public EnumToLocalizedStringMap<NewGameType> newGame => _newGame ?? (_newGame = new EnumToLocalizedStringMap<NewGameType>());

		public EnumToLocalizedStringMap<LevelUpMessages> levelUp => _levelUp ?? (_levelUp = new EnumToLocalizedStringMap<LevelUpMessages>());

		public EnumToLocalizedStringMap<LeafLevel> leaf => _leaf ?? (_leaf = new EnumToLocalizedStringMap<LeafLevel>());

		public EnumToLocalizedStringMap<RebirthLevel> rebirth => _rebirth ?? (_rebirth = new EnumToLocalizedStringMap<RebirthLevel>());

		public EnumToLocalizedStringMap<AdventureCompletionRank> adventureCompletionRank => _adventureCompletionRank ?? (_adventureCompletionRank = new EnumToLocalizedStringMap<AdventureCompletionRank>());

		public EnumToLocalizedStringMap<GameTooltips> tooltips => _tooltips ?? (_tooltips = new EnumToLocalizedStringMap<GameTooltips>());

		public EnumToLocalizedStringMap<ProceduralNodeType> proceduralNodeNames => _proceduralNodeNames ?? (_proceduralNodeNames = new EnumToLocalizedStringMap<ProceduralNodeType>());

		public LocalizedString levelUpCombined => _levelUpCombined?.localizedString;
	}

	[ProtoContract]
	[UIField]
	public class DeckCreationMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<DeckCreationMessage> _message;

		public EnumToLocalizedStringMap<DeckCreationMessage> message => _message ?? (_message = new EnumToLocalizedStringMap<DeckCreationMessage>());
	}

	[ProtoContract]
	[UIField]
	public class PokerMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<PokerHandType> _shortHand;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<PokerHandType> _hand;

		[ProtoMember(3)]
		[UIField]
		private EnumToLocalizedStringMap<PlayingCardValue> _value;

		[ProtoMember(4)]
		[UIField]
		private EnumToLocalizedStringMap<PlayingCardSuit> _suit;

		[ProtoMember(5)]
		[UIField]
		private EnumToLocalizedStringMap<PlayingCardColor> _color;

		[ProtoMember(6)]
		[UIField]
		private EnumToLocalizedStringMap<PlayingCardColor> _colorSuits;

		public EnumToLocalizedStringMap<PokerHandType> shortHand => _shortHand ?? (_shortHand = new EnumToLocalizedStringMap<PokerHandType>());

		public EnumToLocalizedStringMap<PokerHandType> hand => _hand ?? (_hand = new EnumToLocalizedStringMap<PokerHandType>());

		public EnumToLocalizedStringMap<PlayingCardValue> value => _value ?? (_value = new EnumToLocalizedStringMap<PlayingCardValue>());

		public EnumToLocalizedStringMap<PlayingCardSuit> suit => _suit ?? (_suit = new EnumToLocalizedStringMap<PlayingCardSuit>());

		public EnumToLocalizedStringMap<PlayingCardColor> color => _color ?? (_color = new EnumToLocalizedStringMap<PlayingCardColor>());

		public EnumToLocalizedStringMap<PlayingCardColor> colorSuits => _colorSuits ?? (_colorSuits = new EnumToLocalizedStringMap<PlayingCardColor>());
	}

	[ProtoContract]
	[UIField]
	public class PopupMessages
	{
		[ProtoMember(1)]
		[UIField]
		private EnumToLocalizedStringMap<UIPopupTitle> _title;

		[ProtoMember(2)]
		[UIField]
		private EnumToLocalizedStringMap<UIPopupMessage> _messages;

		[ProtoMember(3)]
		[UIField]
		private EnumToLocalizedStringMap<UIPopupButton> _button;

		public EnumToLocalizedStringMap<UIPopupTitle> title => _title ?? (_title = new EnumToLocalizedStringMap<UIPopupTitle>());

		public EnumToLocalizedStringMap<UIPopupMessage> message => _messages ?? (_messages = new EnumToLocalizedStringMap<UIPopupMessage>());

		public EnumToLocalizedStringMap<UIPopupButton> button => _button ?? (_button = new EnumToLocalizedStringMap<UIPopupButton>());
	}

	[ProtoContract]
	[UIField]
	public class EnumToLocalizedStringMap<K> where K : struct, IConvertible
	{
		[ProtoMember(1)]
		[UIField]
		[UIDeepValueChange]
		[UIHideIf("_hideCommon")]
		private LocalizedStringData.AVariable.LocalizedStringVariable _defaultMessage;

		[ProtoMember(2, OverwriteList = true)]
		[UIField(maxCount = 0)]
		[UIFieldCollectionItem]
		[UIFieldKey(flexibleWidth = 1f)]
		[UIFieldValue(flexibleWidth = 4f)]
		[UIHideIf("_hideCommon")]
		private Dictionary<K, LocalizedStringData.AVariable.LocalizedStringVariable> _messages;

		public LocalizedString this[K key] => (_messages?.GetValueOrDefault(key) ?? _defaultMessage ?? (_defaultMessage = new LocalizedStringData.AVariable.LocalizedStringVariable(default(LocalizedStringData.TableEntryId)))).localizedString;
	}

	[ProtoContract(EnumPassthru = true)]
	public enum UIPopupTitle
	{
		Options,
		ExitGame,
		ResetOptions,
		EndAdventure,
		ReservedKey,
		KeyAlreadyBound,
		HelpLocalize,
		OpenExternalBrowser,
		ConfirmRebirth
	}

	[ProtoContract(EnumPassthru = true)]
	public enum UIPopupMessage
	{
		ExitGame,
		ResetOptions,
		EndAdventure,
		ReservedKey,
		KeyAlreadyBound,
		RightClickToUnbind,
		HelpLocalizeBody,
		OpenDiscordBrowser,
		ConfirmRebirth,
		NewRebirthOptions
	}

	[ProtoContract(EnumPassthru = true)]
	public enum UIPopupButton
	{
		Cancel,
		ExitGame,
		ResetOptions,
		EndAdventure,
		PressDesiredKey,
		LeftClickToBeginBinding,
		HelpLocalize,
		OpenExternalBrowser,
		ConfirmRebirth
	}

	[ProtoContract(EnumPassthru = true)]
	public enum GameTooltips
	{
		HP,
		Shield,
		GameUnlocked,
		ViewMap,
		AdventureUnlocked,
		ViewAbilityDeck,
		ViewAbilityDiscard,
		UpgradeAbilityInDeck,
		RemoveAbilityFromDeck,
		CopyAbilityInDeck,
		ReduceAbilityCostInDeck,
		ViewPlayerCardDiscard,
		ViewEnemyCardDiscard,
		ViewPlayerDeck,
		ViewCharacterTraits,
		Ability,
		Card,
		ViewAdventureDiscard,
		ViewLeaderboard,
		ViewEnemyCardDraw
	}

	private static MessageData _Instance;

	[ProtoMember(1)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Attack")]
	private AttackMessages _attack;

	[ProtoMember(2)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Ability")]
	private AbilityMessages _ability;

	[ProtoMember(3)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Discard")]
	private DiscardMessages _discard;

	[ProtoMember(4)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Pick")]
	private PickMessages _pick;

	[ProtoMember(5)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Target")]
	private TargetMessages _target;

	[ProtoMember(6)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Tutorial")]
	private TutorialMessages _tutorial;

	[ProtoMember(7)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Button")]
	private ButtonMessages _button;

	[ProtoMember(8)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Player")]
	private PlayerMessages _player;

	[ProtoMember(9)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Game")]
	private GameMessages _game;

	[ProtoMember(10)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Deck")]
	private DeckCreationMessages _deck;

	[ProtoMember(11)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Poker")]
	private PokerMessages _poker;

	[ProtoMember(12)]
	[UIField(collapse = UICollapseType.Hide)]
	[UICategory("Popup")]
	private PopupMessages _popup;

	public static MessageData Instance => _Instance ?? (_Instance = DataRef<MessageData>.Search().FirstOrDefault()?.data);

	public AttackMessages attack => _attack ?? (_attack = new AttackMessages());

	public AbilityMessages ability => _ability ?? (_ability = new AbilityMessages());

	public DiscardMessages discard => _discard ?? (_discard = new DiscardMessages());

	public PickMessages pick => _pick ?? (_pick = new PickMessages());

	public TargetMessages target => _target ?? (_target = new TargetMessages());

	public TutorialMessages tutorial => _tutorial ?? (_tutorial = new TutorialMessages());

	public ButtonMessages button => _button ?? (_button = new ButtonMessages());

	public PlayerMessages player => _player ?? (_player = new PlayerMessages());

	public GameMessages game => _game ?? (_game = new GameMessages());

	public DeckCreationMessages deck => _deck ?? (_deck = new DeckCreationMessages());

	public PokerMessages poker => _poker ?? (_poker = new PokerMessages());

	public PopupMessages popup => _popup ?? (_popup = new PopupMessages());

	[ProtoMember(128)]
	public string tags { get; set; }

	public string GetTitle()
	{
		return "Messages";
	}

	public string GetAutomatedDescription()
	{
		return null;
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}
}
