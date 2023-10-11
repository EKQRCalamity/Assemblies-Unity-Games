using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using TMPro;
using UnityEngine;

[ProtoContract]
[UIField]
[Localize]
public class AdventureData : IDataContent
{
	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(SetupAbilityCards))]
	[ProtoInclude(11, typeof(SetupResourceCards))]
	public abstract class ASetupInstruction
	{
		[ProtoMember(1, OverwriteList = true)]
		[UIField]
		[UIFieldCollectionItem]
		[UIDeepValueChange]
		protected List<ASetupCondition> _conditions;

		public bool IsValid(GameState state)
		{
			if (!_conditions.IsNullOrEmpty())
			{
				return _conditions.All((ASetupCondition condition) => condition.IsValid(state));
			}
			return true;
		}

		public virtual IEnumerable<ResourceCard> GeneratePlayerCards(GameState state)
		{
			return null;
		}

		public virtual IEnumerable<ResourceCard> GenerateEnemyCards(GameState state)
		{
			return null;
		}

		protected abstract string _ToString();

		public override string ToString()
		{
			return _conditions.ToStringSmart().SpaceIfNotEmpty() + _ToString();
		}
	}

	[ProtoContract]
	[UIField]
	public class SetupAbilityCards : ASetupInstruction
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<AbilityDeckData> _deck;

		[ProtoMember(2)]
		[UIField]
		private bool _shuffle;

		public DataRef<AbilityDeckData> deck => _deck;

		public bool shuffle => _shuffle;

		protected override string _ToString()
		{
			return "Set Ability Deck To <b>" + (_deck ? EnumUtil.FriendlyName(_deck.data.characterClass) : "") + ": " + _deck.GetFriendlyName() + "</b>" + _shuffle.ToText(" Shuffled");
		}
	}

	[ProtoContract]
	[UIField]
	public class SetupResourceCards : ASetupInstruction
	{
		[ProtoMember(1)]
		[UIField(collapse = UICollapseType.Open)]
		private DataRef<ResourceDeckData> _deck;

		[ProtoMember(2)]
		[UIField]
		private bool _setupEnemyCards;

		[ProtoMember(3)]
		[UIField]
		private bool _shuffled;

		public override IEnumerable<ResourceCard> GeneratePlayerCards(GameState state)
		{
			if (!_setupEnemyCards)
			{
				return _deck.data.cards.Select((PlayingCardType t) => new ResourceCard(t, ProfileManager.options.cosmetic.playingCardDeck)).Shuffled(state.random, _shuffled);
			}
			return null;
		}

		public override IEnumerable<ResourceCard> GenerateEnemyCards(GameState state)
		{
			if (!_setupEnemyCards)
			{
				return null;
			}
			return _deck.data.cards.Select((PlayingCardType t) => new ResourceCard(t, PlayingCardSkinType.Enemy)).Shuffled(state.random, _shuffled);
		}

		protected override string _ToString()
		{
			return "Set Resource Deck To <b>" + _deck.GetFriendlyName() + "</b> for " + _setupEnemyCards.ToText("Enemy", "Player") + _shuffled.ToText(" Shuffled");
		}
	}

	[ProtoContract]
	[UIField]
	[ProtoInclude(10, typeof(TotalExperienceSetupCondition))]
	[ProtoInclude(11, typeof(SelectedClass))]
	public abstract class ASetupCondition
	{
		public abstract bool IsValid(GameState state);
	}

	[ProtoContract]
	[UIField]
	public class TotalExperienceSetupCondition : ASetupCondition
	{
		[ProtoMember(1)]
		[UIField(min = 0, max = 10000)]
		private int _experience;

		[ProtoMember(2)]
		[UIField]
		[DefaultValue(FlagCheckType.LessThanOrEqualTo)]
		private FlagCheckType _comparison = FlagCheckType.LessThanOrEqualTo;

		public override bool IsValid(GameState state)
		{
			return _comparison.Check(ProfileManager.progress.experience.read.totalExperience, _experience);
		}

		public override string ToString()
		{
			return $"If total experience {_comparison.GetText()} {_experience}";
		}
	}

	[ProtoContract]
	[UIField]
	public class SelectedClass : ASetupCondition
	{
		[ProtoMember(1)]
		[UIField]
		private PlayerClass _characterClass;

		public override bool IsValid(GameState state)
		{
			return state.player?.characterClass == _characterClass;
		}

		public override string ToString()
		{
			return "If " + EnumUtil.FriendlyName(_characterClass);
		}
	}

	private const string CAT_CARDS = "Cards";

	private const string CAT_OTHER = "Other";

	[ProtoMember(11)]
	[UIField("Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Other")]
	private LocalizedStringData _nameLocalized;

	[ProtoMember(2)]
	[UIField]
	[UICategory("Other")]
	[UIHorizontalLayout("A")]
	private AdventureBackType _cardBack;

	[ProtoMember(5)]
	[UIField]
	[UICategory("Other")]
	[UIHorizontalLayout("A")]
	private AdventureDeckType _deck;

	[ProtoMember(4, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open, maxCount = 10)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Other")]
	private List<AdventureCard.SelectInstruction> _adventureStartInstructions;

	[ProtoMember(21)]
	[UIField(tooltip = "Generated and presented to player just before adventure start instructions late. Also previewed to the player when an adventure deck is inspected.")]
	[UICategory("Other")]
	[UIHideIf("_hideModifier")]
	private DataRef<ProceduralNodePackData> _modifier;

	[ProtoMember(18, OverwriteList = true)]
	[UIField(collapse = UICollapseType.Open, maxCount = 10, tooltip = "Instructions that run after player mulligan and level up selections.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Other")]
	private List<AdventureCard.SelectInstruction> _adventureStartInstructionsLate;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(maxCount = 0, collapse = UICollapseType.Open, order = 1u)]
	[UIFieldCollectionItem]
	[UICategory("Cards")]
	private List<AdventureCard> _cards;

	[ProtoMember(6, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UICategory("Other")]
	[UIDeepValueChange]
	private HashSet<DataRef<BonusCardData>> _additionalBonuses;

	[ProtoMember(7, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UICategory("Other")]
	[UIDeepValueChange]
	private HashSet<DataRef<BonusCardData>> _excludedBonuses;

	[ProtoMember(8)]
	[UIField("S Rank Completion Time (Seconds)", 0u, null, null, null, null, null, null, false, null, 5, false, null, min = 60, max = 3600)]
	[DefaultValue(300)]
	[UICategory("Other")]
	private int _completionTime = 300;

	[ProtoMember(12)]
	[UIField("Description", 0u, null, null, null, null, null, null, false, null, 5, false, null, collapse = UICollapseType.Open)]
	[UIDeepValueChange]
	[UICategory("Other")]
	private LocalizedStringData _descriptionLocalized;

	[ProtoMember(10, OverwriteList = true)]
	[UIField(tooltip = "Rewards are given to player when this adventure is completed.")]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Other")]
	private List<AReward> _rewards;

	[ProtoMember(13)]
	[UIField]
	[UICategory("Other")]
	[UIHorizontalLayout("Bool", expandWidth = false)]
	private bool _unlockedForDemo;

	[ProtoMember(14, OverwriteList = true)]
	[UIField(maxCount = 0)]
	[UIFieldCollectionItem]
	[UIDeepValueChange]
	[UICategory("Other")]
	private List<ASetupInstruction> _setupInstructions;

	[ProtoMember(16)]
	[UIField]
	[DefaultValue(true)]
	[UICategory("Other")]
	[UIHorizontalLayout("Bool")]
	private bool _trackedByAchievements = true;

	[ProtoMember(17)]
	[UIField("DEV Name", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Used for searching adventures in Unity Editor.")]
	[UICategory("Other")]
	private string _devName;

	[ProtoMember(19)]
	[UIField]
	[UICategory("Other")]
	private TraitRuleset? _traitRulesetOverride;

	[ProtoMember(20)]
	[UIField(validateOnChange = true)]
	[UICategory("Other")]
	private LeaderboardType _leaderboardType;

	[ProtoMember(22)]
	[UIField(min = 0, max = 3650)]
	[UICategory("Other")]
	[UIHideIf("_hideModifier")]
	private int _dailySeedOffset;

	[ProtoMember(23)]
	[UIField(tooltip = "Chance that rewarded ability will belong to character being played.\n<i>Overrides game data setting.</i>", min = 0.01, max = 1)]
	[UICategory("Other")]
	private float? _unlockAbilityForCurrentClassChance;

	[ProtoMember(24)]
	[UIField(tooltip = "Chance that preferred abilities <i>(abilities in deck)</i> will continue to be unlocked after first guaranteed preferred ability is unlocked.\n<i>Overrides game data setting.</i>", min = 0, max = 1)]
	[UICategory("Other")]
	private float? _keepPreferredAbilityChance;

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

	[ProtoMember(9)]
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

	[ProtoMember(15)]
	public string tags { get; set; }

	public LocalizedStringData nameLocalized => _nameLocalized;

	public IEnumerable<AdventureCard> cards
	{
		get
		{
			IEnumerable<AdventureCard> enumerable = _cards;
			return enumerable ?? Enumerable.Empty<AdventureCard>();
		}
	}

	public AdventureBackType cardBack => _cardBack;

	public AdventureDeckType deck => _deck;

	public int count => _cards.Count;

	public List<AdventureCard.SelectInstruction> adventureStartInstructions => _adventureStartInstructions ?? (_adventureStartInstructions = new List<AdventureCard.SelectInstruction>());

	public IEnumerable<AdventureCard.SelectInstruction> adventureStartInstructionsLate
	{
		get
		{
			IEnumerable<AdventureCard.SelectInstruction> enumerable = _adventureStartInstructionsLate;
			return enumerable ?? Enumerable.Empty<AdventureCard.SelectInstruction>();
		}
	}

	public IEnumerable<DataRef<BonusCardData>> additionalBonuses
	{
		get
		{
			IEnumerable<DataRef<BonusCardData>> enumerable = _additionalBonuses;
			return enumerable ?? Enumerable.Empty<DataRef<BonusCardData>>();
		}
	}

	public IEnumerable<DataRef<BonusCardData>> excludedBonuses
	{
		get
		{
			IEnumerable<DataRef<BonusCardData>> enumerable = _excludedBonuses;
			return enumerable ?? Enumerable.Empty<DataRef<BonusCardData>>();
		}
	}

	public bool unlockedForDemo => _unlockedForDemo;

	public bool trackedByAchievements => _trackedByAchievements;

	public TraitRuleset? traitRulesetOverride => _traitRulesetOverride;

	public bool dailyLeaderboardEnabled => _leaderboardType == LeaderboardType.Daily;

	public int dailySeedOffset => _dailySeedOffset;

	public DataRef<ProceduralNodePackData> modifier => _modifier;

	public float? unlockAbilityForCurrentClassChance => _unlockAbilityForCurrentClassChance;

	public float? keepPreferredAbilityChance => _keepPreferredAbilityChance;

	private bool _hideModifier => !dailyLeaderboardEnabled;

	private bool _modifierSpecified => _modifier.ShouldSerialize();

	private int _GetCompletionTime(NewGameType? newGameType = null)
	{
		return Mathf.RoundToInt((float)_completionTime * (newGameType ?? GameState.NewGame).GetCompletionTimeMultiplier());
	}

	public IEnumerable<ATarget> GenerateCards(GameState state)
	{
		if ((bool)modifier && !state.modifierNode)
		{
			DataRef<ProceduralNodeData> node = modifier.data.pack.GetSelection(state.random).node;
			if (node != null && (bool)node)
			{
				DataRef<ProceduralNodeData> dataRef2 = (state.modifierNode = node);
				foreach (ATarget item in dataRef2.data.cards.GenerateCards(state))
				{
					yield return item;
				}
			}
		}
		foreach (ATarget item2 in _cards.GenerateCards(state))
		{
			yield return item2;
		}
	}

	public AdventureCompletionRank GetCompletionRank(int strategyTime, NewGameType? newGameType = null)
	{
		AdventureCompletionRank[] values = EnumUtil<AdventureCompletionRank>.Values;
		foreach (AdventureCompletionRank adventureCompletionRank in values)
		{
			if (strategyTime <= adventureCompletionRank.GetCompletionTime(_GetCompletionTime(newGameType)))
			{
				return adventureCompletionRank;
			}
		}
		return EnumUtil<AdventureCompletionRank>.Max;
	}

	public int GetCompletionTime(AdventureCompletionRank rank, NewGameType? newGameType = null)
	{
		return rank.GetCompletionTime(_GetCompletionTime(newGameType));
	}

	public AdventureCompletionRank? GetNextCompletionRank(int strategyTime)
	{
		if (!EnumUtil.HasPrevious(GetCompletionRank(strategyTime)))
		{
			return null;
		}
		return EnumUtil.Previous(GetCompletionRank(strategyTime));
	}

	public IEnumerable<AReward> GetRewardsThatShouldUnlock()
	{
		if (!_rewards.IsNullOrEmpty())
		{
			return _rewards.Where((AReward reward) => reward.ShouldUnlock());
		}
		return Enumerable.Empty<AReward>();
	}

	public int IndexOf(AdventureCard card)
	{
		return _cards?.IndexOf(card) ?? (-1);
	}

	public void ReplaceAtIndex(AdventureCard newCard, int index)
	{
		_cards[index] = newCard;
	}

	public PoolKeepItemListHandle<ASetupInstruction> GetValidSetupInstructions(GameState state)
	{
		PoolKeepItemListHandle<ASetupInstruction> poolKeepItemListHandle = Pools.UseKeepItemList<ASetupInstruction>();
		if (!_setupInstructions.IsNullOrEmpty())
		{
			foreach (ASetupInstruction setupInstruction in _setupInstructions)
			{
				if (setupInstruction.IsValid(state))
				{
					poolKeepItemListHandle.Add(setupInstruction);
				}
			}
			return poolKeepItemListHandle;
		}
		return poolKeepItemListHandle;
	}

	public string GetTitle()
	{
		if (LaunchManager.InGame || !_devName.HasVisibleCharacter())
		{
			return _nameLocalized ?? (_nameLocalized = new LocalizedStringData(""));
		}
		return _devName;
	}

	public string GetAutomatedDescription()
	{
		return _descriptionLocalized ?? (_descriptionLocalized = new LocalizedStringData(""));
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		_additionalBonuses?.RemoveWhere((DataRef<BonusCardData> d) => !d);
		_excludedBonuses?.RemoveWhere((DataRef<BonusCardData> d) => !d);
	}

	public string GetSaveErrorMessage()
	{
		return null;
	}

	public void OnLoadValidation()
	{
	}

	[UIField]
	[UICategory("Cards")]
	private void _ConvertEnemiesToNewGameVersions()
	{
		if (!(DataRefControl.ActiveControl?.data is AdventureData))
		{
			return;
		}
		UIUtil.CreatePopup("Convert All Enemies To New Game Versions", UIUtil.CreateMessageBox("Would you like to convert all enemies in this adventure to new game versions?", TextAlignmentOptions.Center, 32, 1600), null, parent: DataRefControl.ActiveControl.transform, buttons: new string[2] { "Convert", "Cancel" }, size: null, centerReferece: null, center: null, pivot: null, onClose: null, displayCloseButton: true, blockAllRaycasts: true, resourcePath: null, onButtonClick: delegate(string s)
		{
			if (s == "Convert")
			{
				for (int i = 0; i < _cards.Count; i++)
				{
					if (_cards[i] is AdventureCard.Enemy enemy)
					{
						enemy.ReplaceWithNewGameVersion(this, i);
					}
					else if (_cards[i] is AdventureCard.NewGame newGame && newGame.GetOverride(NewGameType.Spring) is AdventureCard.Enemy enemy2 && newGame.HasEmptySlot())
					{
						enemy2.ReplaceWithNewGameVersion(this, i);
					}
				}
				DataRefControl.ActiveControl.Refresh();
			}
		});
	}
}
