using System;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public sealed class Player : ACombatant, IComparable<Player>
{
	[ProtoMember(1)]
	private DataRef<CharacterData> _characterDataRef;

	[ProtoMember(2)]
	private PlayerStatistics _playerStats;

	[ProtoMember(3)]
	private BInt _numberOfHeroAbilities;

	public DataRef<CharacterData> characterDataRef => _characterDataRef;

	public CharacterData characterData => _characterDataRef.data;

	public PlayerClass characterClass => characterData.characterClass;

	public override IdDeck<Ability.Pile, Ability> abilityDeck => base.gameState.abilityDeck;

	public IdDeck<HeroDeckPile, Ability> heroDeck => base.gameState.heroDeck;

	public PlayerStatistics playerStats => _playerStats ?? (_playerStats = new PlayerStatistics());

	public BInt numberOfHeroAbilities
	{
		get
		{
			return _numberOfHeroAbilities ?? (_numberOfHeroAbilities = new BInt());
		}
		set
		{
			_numberOfHeroAbilities = value;
		}
	}

	public PlayerCardView playerCard => base.view as PlayerCardView;

	public int level => ProfileManager.progress.experience.read.GetEffectiveLevel(characterDataRef);

	public RebirthLevel rebirth => (RebirthLevel)ProfileManager.progress.experience.read.GetRebirth(characterDataRef);

	public int abilityHandSpace => Math.Max(0, (int)playerStats[PlayerStatType.AbilityHandSize] - abilityDeck.Count(Ability.Pile.Hand, Ability.IsPermanent));

	public int handSpace => Math.Max(0, (int)playerStats[PlayerStatType.ResourceHandSize] - resourceDeck.Count(ResourceCard.Pile.Hand, ResourceCard.IsPermanent));

	public override int registerDuringGameStateInitializationOrder => 0;

	public override string name => characterData.name;

	public override string description => combatantData.traits.ToStringSmart(" & ");

	public override CroppedImageRef image => null;

	public override ResourceBlueprint<GameObject> blueprint => PlayerCardView.Blueprint;

	public override Faction faction => Faction.Player;

	public override IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck => base.gameState.playerResourceDeck;

	public override CombatantData combatantData => characterData;

	public override bool canAct
	{
		get
		{
			if (!base.dead && !base.tapped)
			{
				if (!canAttack)
				{
					return HasAbilityThatCanActivate();
				}
				return true;
			}
			return false;
		}
	}

	protected override PokerHandTypes _canAttackWith => Poker.ALL_HANDS_EXCEPT_TWO_PAIR;

	protected override PokerHandTypes _canDefendWidth => Poker.ALL_HANDS_EXCEPT_TWO_PAIR;

	public Player()
	{
	}

	public Player(DataRef<CharacterData> characterDataRef)
	{
		_characterDataRef = characterDataRef;
		_playerStats = new PlayerStatistics(5, 5, 1, 1);
		_Initialize(characterData);
		characterDataRef.data.ApplyLevelUps(this, level);
		base.HP.value = base.stats.health;
		numberOfHeroAbilities.value = 1;
	}

	private void _DrawToFullResourceHand()
	{
		GameStepStack stack = base.gameState.stack;
		IdDeck<ResourceCard.Pile, ResourceCard> idDeck = resourceDeck;
		int size = playerStats[PlayerStatType.ResourceHandSize];
		Func<ResourceCard, bool> isPermanent = ResourceCard.IsPermanent;
		stack.Push(idDeck.DrawToSizeStep(size, null, null, null, 0.1f, isPermanent));
	}

	private void _CheckResourceHandOverMaxSize(ResourceCard value, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if (newPile == ResourceCard.Pile.Hand)
		{
			int num = resourceDeck.Count(ResourceCard.Pile.Hand, ResourceCard.IsPermanent);
			if (num > (int)playerStats[PlayerStatType.ResourceHandSize])
			{
				base.gameState.stack.Push(new GameStepDiscardResourceChoice(num - (int)playerStats[PlayerStatType.ResourceHandSize], DiscardReason.HandFull));
			}
		}
	}

	private void _OnResourceMaxSizeChange(int previousSize, int size)
	{
		_CheckResourceHandOverMaxSize(null, null, ResourceCard.Pile.Hand);
	}

	private void _OnAbilityTransfer(Ability value, Ability.Pile? oldPile, Ability.Pile? newPile)
	{
		if (newPile.RegistersReactions())
		{
			value.Register();
		}
		else if (oldPile.RegistersReactions())
		{
			value.Unregister();
		}
		if (newPile == Ability.Pile.Discard || !newPile.HasValue)
		{
			value.UnregisterRefreshTargetEvents();
			value.tapped.value = false;
			value.hasTakenTurn.value = false;
		}
		_CheckAbilityHandOverMaxSize(value, oldPile, newPile);
	}

	private void _CheckAbilityHandOverMaxSize(Ability value, Ability.Pile? oldPile, Ability.Pile? newPile)
	{
		if (newPile == Ability.Pile.Hand)
		{
			int num = abilityDeck.Count(Ability.Pile.Hand, Ability.IsPermanent);
			if (num > (int)playerStats[PlayerStatType.AbilityHandSize])
			{
				base.gameState.stack.Push(new GameStepDiscardAbilityChoice(num - (int)playerStats[PlayerStatType.AbilityHandSize], DiscardReason.HandFull));
			}
		}
	}

	private void _OnAbilityMaxSizeChange(int previousSize, int size)
	{
		_CheckAbilityHandOverMaxSize(null, null, Ability.Pile.Hand);
	}

	public override void _Register()
	{
		base._Register();
		resourceDeck.onTransfer += _CheckResourceHandOverMaxSize;
		playerStats[PlayerStatType.ResourceHandSize].onValueChanged += _OnResourceMaxSizeChange;
		abilityDeck.onTransfer += _OnAbilityTransfer;
		playerStats[PlayerStatType.AbilityHandSize].onValueChanged += _OnAbilityMaxSizeChange;
	}

	public override void _Unregister()
	{
		resourceDeck.onTransfer -= _CheckResourceHandOverMaxSize;
		playerStats[PlayerStatType.ResourceHandSize].onValueChanged -= _OnResourceMaxSizeChange;
		abilityDeck.onTransfer -= _OnAbilityTransfer;
		playerStats[PlayerStatType.AbilityHandSize].onValueChanged -= _OnAbilityMaxSizeChange;
		base._Unregister();
	}

	public override AGameStepTurn GetTurnStep()
	{
		return new GameStepTurnPlayer(this);
	}

	public override GameStep GetDefenseStep()
	{
		return new GameStepPresentDefensePlayer(this);
	}

	public override void OnEncounterStart()
	{
		base.OnEncounterStart();
		_DrawToFullResourceHand();
	}

	public override void OnEncounterEnd()
	{
		base.OnEncounterEnd();
		base.shield.value = 0;
		base.numberOfAttacks.value = 0;
		numberOfHeroAbilities.value = 1;
		_DrawToFullResourceHand();
	}

	public override void OnRoundStart()
	{
		base.OnRoundStart();
		numberOfHeroAbilities.value = playerStats[PlayerStatType.NumberOfHeroAbilities];
		foreach (Ability card in abilityDeck.GetCards(Ability.Pile.HeroAct))
		{
			if (card is ItemCard itemCard)
			{
				itemCard.hasBeenUsedThisRound = false;
			}
		}
	}

	public override void OnTurnStart()
	{
		base.OnTurnStart();
		_DrawToFullResourceHand();
		base.gameState.stack.Push(abilityDeck.DrawStep(Math.Min(playerStats[PlayerStatType.AbilityDrawCount], abilityHandSpace)));
	}

	public override void OnTurnEnd()
	{
		_DrawToFullResourceHand();
		base.OnTurnEnd();
	}

	public bool HasAbilityThatCanActivate()
	{
		return abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct).Any((Ability ability) => ability.CanActivate());
	}

	public bool HasAbilityThatCanActivateWhichRequiresCards()
	{
		return abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct).Any((Ability ability) => ability.cost.GetResourceFilters().Any() && (bool)ability.CanActivate());
	}

	public bool HasAbilityThatCanActivateWhichUsesCard(ResourceCard card, ResourceCard.Piles activationPilesOverride = ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand)
	{
		return abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct).Any((Ability ability) => ability.cost.GetResourceFilters().Any((PlayingCard.Filter filter) => filter.AreValid(card)) && (bool)ability.CanActivate(activationPilesOverride));
	}

	public bool HasAbilityThatCanActivateWhichCouldUseCardButIsNotCurrently(ResourceCard card, ResourceCard.Piles activationPilesOverride = ResourceCard.Piles.Hand | ResourceCard.Piles.ActivationHand | ResourceCard.Piles.AttackHand | ResourceCard.Piles.DefenseHand)
	{
		return abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct).Any(delegate(Ability ability)
		{
			if (ability.cost.GetResourceFilters().Any((PlayingCard.Filter filter) => filter.AreValid(card)) && (bool)ability.CanActivate(activationPilesOverride))
			{
				PoolKeepItemListHandle<ResourceCard> activationCards = ability.GetActivationCards();
				if (activationCards == null)
				{
					return true;
				}
				return !activationCards.AsEnumerable().Contains(card);
			}
			return false;
		});
	}

	public bool HasReactionWithTarget()
	{
		return abilityDeck.GetCards(Ability.Piles.Hand | Ability.Piles.HeroAct).Any((Ability ability) => ability.hasActiveReaction && ability.GetTargets().Any());
	}

	public void ApplyLevelUpsToGameState()
	{
		characterData.ApplyLevelUps(base.gameState, level);
	}

	public PoolKeepItemHashSetHandle<DataRef<AbilityData>> GetLockedLevelUpAbilities()
	{
		PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet<DataRef<AbilityData>>();
		foreach (DataRef<AbilityData> item in characterData.AbilitiesUnlockedByLevelUps())
		{
			poolKeepItemHashSetHandle.Add(item);
		}
		foreach (DataRef<AbilityData> item2 in characterData.AbilitiesUnlockedByLevelUps(level))
		{
			poolKeepItemHashSetHandle.Remove(item2);
		}
		return poolKeepItemHashSetHandle;
	}

	public int GetDrawCount(int drawCount, bool canOverdraw = false)
	{
		if (!canOverdraw)
		{
			return Math.Min(drawCount, handSpace);
		}
		return drawCount;
	}

	public GameStep DrawStep(int drawCount, ResourceCard.Pile? drawFrom = null, bool canOverdraw = false)
	{
		return resourceDeck.DrawStep(GetDrawCount(drawCount, canOverdraw), drawFrom);
	}

	public int CompareTo(Player other)
	{
		return characterClass.SortOrder() - other.characterClass.SortOrder();
	}

	public override int CompareTo(ATarget other)
	{
		if (!(other is Player other2))
		{
			return base.CompareTo(other);
		}
		return CompareTo(other2);
	}
}
