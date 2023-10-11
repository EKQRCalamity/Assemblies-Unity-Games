using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(15, typeof(Enemy))]
[ProtoInclude(16, typeof(Player))]
public abstract class ACombatant : AEntity, IAdventureCard
{
	[ProtoContract]
	public class CombatData
	{
		[ProtoMember(1)]
		public ModifiableFlags<PokerHandTypes> canAttackWith { get; private set; }

		[ProtoMember(2)]
		public ModifiableFlags<PokerHandTypes> canBeDefendedWith { get; private set; }

		[ProtoMember(3)]
		public ModifiableFlags<PokerHandTypes> canDefendWith { get; private set; }

		[ProtoMember(4)]
		public ModifiableFlags<PokerHandTypes> canBeAttackedWith { get; private set; }

		[ProtoMember(5)]
		public int attackTieBreaker { get; set; }

		[ProtoMember(6)]
		public int defenseTieBreaker { get; set; }

		[ProtoMember(7)]
		public int attackOffset { get; set; }

		[ProtoMember(8)]
		public int defenseOffset { get; set; }

		public ModifiableFlags<PokerHandTypes> this[CombatPokerHandTypes type] => type switch
		{
			CombatPokerHandTypes.CanAttackWith => canAttackWith, 
			CombatPokerHandTypes.CanBeDefendedWith => canBeDefendedWith, 
			CombatPokerHandTypes.CanDefendWith => canDefendWith, 
			CombatPokerHandTypes.CanBeAttackedWith => canBeAttackedWith, 
			_ => null, 
		};

		public int this[CombatType combatType]
		{
			get
			{
				return combatType switch
				{
					CombatType.Attack => attackTieBreaker, 
					CombatType.Defense => defenseTieBreaker, 
					_ => 0, 
				};
			}
			set
			{
				switch (combatType)
				{
				case CombatType.Attack:
					attackTieBreaker = value;
					break;
				case CombatType.Defense:
					defenseTieBreaker = value;
					break;
				}
			}
		}

		private CombatData()
		{
		}

		public CombatData(PokerHandTypes canAttackWith, PokerHandTypes canBeDefendedWith, PokerHandTypes canDefendWith, PokerHandTypes canBeAttackedWith)
		{
			this.canAttackWith = new ModifiableFlags<PokerHandTypes>(canAttackWith);
			this.canBeDefendedWith = new ModifiableFlags<PokerHandTypes>(canBeDefendedWith);
			this.canDefendWith = new ModifiableFlags<PokerHandTypes>(canDefendWith);
			this.canBeAttackedWith = new ModifiableFlags<PokerHandTypes>(canBeAttackedWith);
		}
	}

	public const int MAX_ATTACKS = 10;

	[ProtoMember(1)]
	private Statistics _stats;

	[ProtoMember(2)]
	private BInt _HP;

	[ProtoMember(3)]
	private CappedBInt _shield;

	[ProtoMember(4)]
	private CappedBInt _numberOfAttacks;

	[ProtoMember(5, OverwriteList = true)]
	private List<Id<Ability>> _traits;

	[ProtoMember(6, OverwriteList = true)]
	private IdDeck<AppliedPile, Ability> _appliedAbilities;

	[ProtoMember(7, OverwriteList = true)]
	private IdDeck<AppliedPile, ResourceCard> _appliedResources;

	[ProtoMember(8)]
	private Statuses _statuses;

	[ProtoMember(9)]
	private CombatData _combat;

	[ProtoMember(10)]
	private bool _dieing;

	private Func<List<ResourceCard>, bool> _isValidAutoDefenseFill;

	public TopDeckResult? activeTopDeckResult;

	public Statistics stats => _stats ?? (_stats = new Statistics());

	public Statuses statuses => _statuses ?? (_statuses = new Statuses());

	public CombatData combat => _combat ?? (_combat = new CombatData(_canAttackWith, _canBeDefendedWith, _canDefendWidth, _canBeAttackedWith));

	public BInt HP
	{
		get
		{
			return _HP ?? (_HP = new BInt());
		}
		set
		{
			_HP = value;
		}
	}

	public int HPMissing => Math.Max(0, (int)stats.health - (int)HP);

	public CappedBInt shield
	{
		get
		{
			return _shield ?? (_shield = new CappedBInt());
		}
		set
		{
			_shield = value;
		}
	}

	public CappedBInt numberOfAttacks
	{
		get
		{
			return _numberOfAttacks ?? (_numberOfAttacks = new CappedBInt(0, 10));
		}
		set
		{
			_numberOfAttacks = value;
		}
	}

	private List<Id<Ability>> traits => _traits ?? (_traits = new List<Id<Ability>>());

	public IdDeck<AppliedPile, Ability> appliedAbilities => _appliedAbilities ?? (_appliedAbilities = new IdDeck<AppliedPile, Ability>());

	public IdDeck<AppliedPile, ResourceCard> appliedResources => _appliedResources ?? (_appliedResources = new IdDeck<AppliedPile, ResourceCard>());

	public sealed override bool canAttack => new CanAttackResult(this);

	public override bool canBeAttacked => new CanAttackResult(null, this);

	public bool isAttacking => base.gameState.activeCombat?.attacker == this;

	public bool isPreparingAttack
	{
		get
		{
			if (isAttacking)
			{
				return !base.gameState.activeCombat.attackHasBeenLaunched;
			}
			return false;
		}
	}

	public bool isDefending => base.gameState.activeCombat?.defender == this;

	public bool isPreparingDefense
	{
		get
		{
			if (isDefending && base.gameState.activeCombat.attackHasBeenLaunched)
			{
				return !base.gameState.activeCombat.defenseHasBeenLaunched;
			}
			return false;
		}
	}

	public bool dieing
	{
		get
		{
			return _dieing;
		}
		set
		{
			_dieing = value;
		}
	}

	public CombatType? activeCombatType
	{
		get
		{
			if (!isAttacking)
			{
				if (!isDefending)
				{
					return null;
				}
				return CombatType.Defense;
			}
			return CombatType.Attack;
		}
	}

	public EntityAudioData audio => combatantData.audio;

	public CombatMediaData combatMedia => combatantData.combatMedia;

	public CombatantCardView combatantCard => base.view as CombatantCardView;

	private Func<List<ResourceCard>, bool> isValidAutoDefenseFill => _IsValidAutoDefenseFill;

	public abstract IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck { get; }

	public abstract IdDeck<Ability.Pile, Ability> abilityDeck { get; }

	public abstract CombatantData combatantData { get; }

	public PokerHandType[] attackHandOrder => Poker.PokerHandsDescendingSize;

	protected virtual PokerHandTypes _canAttackWith => Poker.ALL_HANDS_EXCEPT_TWO_PAIR_AND_FIVE_CARD;

	protected virtual PokerHandTypes _canBeDefendedWith => EnumUtil<PokerHandTypes>.AllFlags;

	private PokerHandType[] defenseHandOrder => Poker.PokerHandsFindDefenseOrderReverse;

	protected virtual PokerHandTypes _canDefendWidth => Poker.ALL_HANDS_EXCEPT_TWO_PAIR_AND_FIVE_CARD;

	protected virtual PokerHandTypes _canBeAttackedWith => EnumUtil<PokerHandTypes>.AllFlags;

	public override bool deadOrDieing
	{
		get
		{
			if (!base.deadOrDieing && !dieing)
			{
				return (int)HP <= 0;
			}
			return true;
		}
	}

	public override bool deadOrInsuredDeath
	{
		get
		{
			if (!base.deadOrInsuredDeath)
			{
				return dieing;
			}
			return true;
		}
	}

	public override bool shouldRegisterDuringGameStateInitialization => base.pile == AdventureCard.Pile.TurnOrder;

	public override bool canUntap => !statuses.cannotUntap;

	public ATarget adventureCard => this;

	public AdventureCard.Pile pileToTransferToOnSelect => AdventureCard.Pile.TurnOrder;

	public virtual AdventureCard.Pile pileToTransferToOnDraw => AdventureCard.Pile.TurnOrder;

	public GameStep selectTransferStep => base.gameState.adventureDeck.TransferCardStep(this, AdventureCard.Pile.TurnOrder);

	[ProtoMember(14)]
	public AdventureCard.Common adventureCardCommon { get; set; }

	public virtual string name => adventureCardCommon?.name;

	public virtual string description => adventureCardCommon?.description;

	public virtual CroppedImageRef image => adventureCardCommon?.image;

	public IEnumerable<GameStep> selectedGameSteps
	{
		get
		{
			yield break;
		}
	}

	public virtual ResourceBlueprint<GameObject> blueprint => EnemyCardView.Blueprint;

	public Ability this[AppliedPile appliedPile] => _appliedAbilities.GetCards(appliedPile).FirstOrDefault();

	protected void _Initialize(CombatantData data)
	{
		_stats = new Statistics(data.stats[StatType.Offense], data.stats[StatType.Defense], data.stats[StatType.Health], data.stats[StatType.NumberOfAttacks], data.stats[StatType.ShieldRetention]);
		HP.value = stats.health;
	}

	private void _OnHPChange(int previousHP, int newHP)
	{
		base.gameState.SignalHPChange(this, previousHP, newHP);
	}

	private void _OnShieldChange(int previousShield, int newShield)
	{
		base.gameState.SignalShieldChange(this, previousShield, newShield);
	}

	private void _OnOffenseChange(int oldStat, int newStat)
	{
		base.gameState.SignalStatChange(this, StatType.Offense, oldStat, newStat);
	}

	private void _OnDefenseChange(int oldStat, int newStat)
	{
		base.gameState.SignalStatChange(this, StatType.Defense, oldStat, newStat);
	}

	private void _OnHealthChange(int oldStat, int newStat)
	{
		base.gameState.SignalStatChange(this, StatType.Health, oldStat, newStat);
	}

	private void _OnNumberOfAttacksChange(int oldStat, int newStat)
	{
		base.gameState.SignalStatChange(this, StatType.NumberOfAttacks, oldStat, newStat);
	}

	private bool _IsValidAutoDefenseFill(List<ResourceCard> cards)
	{
		foreach (ResourceCard card in resourceDeck.GetCards(ResourceCard.Pile.DefenseHand))
		{
			if (!cards.Contains(card))
			{
				return false;
			}
		}
		return true;
	}

	private void _SortActiveBaseTraits()
	{
		using PoolKeepItemDictionaryHandle<DataRef<AbilityData>, Id<Ability>> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<DataRef<AbilityData>, Id<Ability>>();
		for (int num = traits.Count - 1; num >= 0; num--)
		{
			if (combatantData.traits.Contains(traits[num].value.dataRef) && poolKeepItemDictionaryHandle.value.TryAdd(traits[num].value.dataRef, traits[num]))
			{
				traits.RemoveAt(num);
			}
		}
		int num2 = 0;
		foreach (DataRef<AbilityData> trait in combatantData.traits)
		{
			if (poolKeepItemDictionaryHandle.ContainsKey(trait))
			{
				traits.Insert(num2++, poolKeepItemDictionaryHandle[trait]);
			}
		}
	}

	public virtual PokerHandTypes CanAttackWith(ACombatant defender = null)
	{
		PokerHandTypes num = combat.canAttackWith;
		ModifiableFlags<PokerHandTypes> modifiableFlags = defender?.combat.canBeAttackedWith;
		return num & ((modifiableFlags != null) ? ((PokerHandTypes)modifiableFlags) : EnumUtil<PokerHandTypes>.AllFlags);
	}

	public virtual int GetOffenseAgainst(ACombatant defender, bool shouldTriggerMedia = false)
	{
		return (int)stats[StatType.Offense] - (int)defender.stats[StatType.Defense];
	}

	public virtual CanAttackResult CanAttack(ACombatant defender)
	{
		return new CanAttackResult(this, defender);
	}

	public virtual bool CanFormAttack(IEnumerable<ResourceCard> cards, ACombatant defender = null, IEnumerable<ResourceCard> cardsToFreeze = null)
	{
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(cards);
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType) attackCombatHand = GetAttackCombatHand(poolKeepItemListHandle.value, defender, cardsToFreeze);
		return attackCombatHand.Item1?.Count == poolKeepItemListHandle.Count && attackCombatHand.Item1.IfNotNullDispose();
	}

	public PokerHand GetAttackHand(ACombatant defender = null)
	{
		return resourceDeck.GetCards(ResourceCard.Pile.AttackHand).GetPokerHand(CanAttackWith(defender)).Offset(combat.attackOffset);
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetAttackCombatHand(IEnumerable<ResourceCard> cards, ACombatant defender = null, IEnumerable<ResourceCard> cardsToFreeze = null, bool wildIntoPokerHand = false)
	{
		using (cardsToFreeze.FreezeWildValues())
		{
			(PoolKeepItemListHandle<ResourceCard>, PokerHandType) combatHand = cards.GetCombatHand(CanAttackWith(defender), attackHandOrder);
			if (wildIntoPokerHand)
			{
				combatHand.WildIntoPokerHand(disposeHand: false);
			}
			return combatHand;
		}
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetAttackCombatHand(ResourceCard.Pile pile, ACombatant defender = null, IEnumerable<ResourceCard> cardsToFreeze = null, bool wildIntoPokerHand = false)
	{
		return GetAttackCombatHand(resourceDeck.GetCards(pile), defender, cardsToFreeze, wildIntoPokerHand);
	}

	public virtual PokerHandTypes CanDefendWith(ACombatant attacker = null, PokerHand attackHand = null)
	{
		PokerHandTypes num = combat.canDefendWith;
		ModifiableFlags<PokerHandTypes> modifiableFlags = attacker?.combat.canBeDefendedWith;
		return num & ((modifiableFlags != null) ? ((PokerHandTypes)modifiableFlags) : EnumUtil<PokerHandTypes>.AllFlags) & base.gameState.ProcessDefenseRules(attacker, this, attackHand) & EnumUtil<PokerHandTypes>.AllFlagsGreaterThanOrEqualToConvert(attackHand?.type ?? EnumUtil<PokerHandType>.Min);
	}

	public virtual int GetDefenseAgainst(ACombatant attacker, bool shouldTriggerMedia = false)
	{
		return (int)stats[StatType.Defense] - (int)attacker.stats[StatType.Offense];
	}

	public virtual DefenseResultType CanFormDefense(IEnumerable<ResourceCard> defenseCards, ACombatant attacker, PokerHand attackHand, IEnumerable<ResourceCard> cardsToFreeze = null)
	{
		if (attackHand == null)
		{
			return DefenseResultType.Invalid;
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = Pools.UseKeepItemList(defenseCards);
		if (poolKeepItemListHandle.Count == 0)
		{
			return DefenseResultType.Invalid;
		}
		using (cardsToFreeze.FreezeWildValues())
		{
			(PoolKeepItemListHandle<ResourceCard>, PokerHandType) defenseCombatHand = GetDefenseCombatHand(poolKeepItemListHandle.value, attacker, attackHand, Poker.GetHandTypesBySize(poolKeepItemListHandle.Count));
			if (!defenseCombatHand.Item1)
			{
				return (poolKeepItemListHandle.Count == attackHand.hand.Count) ? DefenseResultType.Failure : DefenseResultType.Invalid;
			}
			using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle2 = defenseCombatHand.Item1;
			if (poolKeepItemListHandle.Count > poolKeepItemListHandle2.Count)
			{
				return DefenseResultType.Invalid;
			}
			using (ResourceCard.WildSnapshot.Create(poolKeepItemListHandle2.value))
			{
				defenseCombatHand.WildIntoPokerHand(disposeHand: false);
				DefenseResultType defenseResultType = attackHand.GetAttackResultType(defenseCombatHand.Item1.value.GetPokerHand(CanDefendWith(attacker, attackHand)).Offset(combat.defenseOffset)).GetDefenseResultType();
				if (defenseResultType == DefenseResultType.Tie)
				{
					switch (GetDefenseTieBreaker(attacker))
					{
					case 1:
						return DefenseResultType.Success;
					case -1:
						return DefenseResultType.Failure;
					}
				}
				return defenseResultType;
			}
		}
	}

	public PokerHand GetDefenseHand(ACombatant attacker = null)
	{
		return resourceDeck.GetCards(ResourceCard.Pile.DefenseHand).GetPokerHand(CanDefendWith(attacker, attacker?.GetAttackHand())).Offset(combat.defenseOffset);
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetDefenseCombatHand(IEnumerable<ResourceCard> cards, ACombatant attacker = null, PokerHand attackHand = null, PokerHandTypes? additionalHandFilters = null, IEnumerable<ResourceCard> cardsToFreeze = null)
	{
		using (cardsToFreeze.FreezeWildValues())
		{
			return cards.GetCombatHand(CanDefendWith(attacker, attackHand ?? attacker?.GetAttackHand()) & (additionalHandFilters ?? EnumUtil<PokerHandTypes>.AllFlags), defenseHandOrder);
		}
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetDefenseCombatHand(ResourceCard.Pile pile, ACombatant attacker = null, PokerHand attackHand = null, IEnumerable<ResourceCard> cardsToFreeze = null)
	{
		return GetDefenseCombatHand(resourceDeck.GetCards(pile), attacker, attackHand, null, cardsToFreeze);
	}

	public int GetDefenseTieBreaker(ACombatant attacker)
	{
		return Math.Sign(Math.Sign(combat.defenseTieBreaker) - Math.Sign(attacker.combat.attackTieBreaker));
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType, DefenseResultType result) FindDefenseHandAgainstAllowInvalid(ACombatant attacker, IEnumerable<ResourceCard> cardsToFreeze = null)
	{
		using (cardsToFreeze.FreezeWildValues())
		{
			PokerHand attackHand = attacker.GetAttackHand(this);
			(PoolKeepItemListHandle<ResourceCard>, PokerHandType, DefenseResultType) defenseHand = resourceDeck.GetCards(ResourceCard.Piles.Hand | ResourceCard.Piles.DefenseHand).GetDefenseHand(attackHand, CanDefendWith(attacker, attackHand), GetDefenseTieBreaker(attacker), combat.defenseOffset, (resourceDeck.Count(ResourceCard.Pile.DefenseHand) == 0) ? null : isValidAutoDefenseFill);
			if ((bool)defenseHand.Item1)
			{
				return defenseHand;
			}
			defenseHand.Item1 = Pools.UseKeepItemList(resourceDeck.GetCards(ResourceCard.Pile.Hand).Take(attackHand.hand.Count - resourceDeck.Count(ResourceCard.Pile.DefenseHand)));
			defenseHand.Item3 = DefenseResultType.Failure;
			return defenseHand;
		}
	}

	public abstract GameStep GetDefenseStep();

	public override void _Register()
	{
		HP.onValueChanged += _OnHPChange;
		shield.onValueChanged += _OnShieldChange;
		stats.offense.onValueChanged += _OnOffenseChange;
		stats.defense.onValueChanged += _OnDefenseChange;
		stats.health.onValueChanged += _OnHealthChange;
		stats.numberOfAttacks.onValueChanged += _OnNumberOfAttacksChange;
		if (!this.IsRegistered())
		{
			AddBaseTraits();
		}
	}

	public override void _Unregister()
	{
		HP.onValueChanged -= _OnHPChange;
		shield.onValueChanged -= _OnShieldChange;
		stats.offense.onValueChanged -= _OnOffenseChange;
		stats.defense.onValueChanged -= _OnDefenseChange;
		stats.health.onValueChanged -= _OnHealthChange;
		stats.numberOfAttacks.onValueChanged -= _OnNumberOfAttacksChange;
		RemoveTraits();
		shield.value = 0;
		base.activeTap.value = false;
	}

	public override void OnEncounterStart()
	{
		numberOfAttacks.value = stats[StatType.NumberOfAttacks];
	}

	public override void OnTurnStart()
	{
		GameStateView gameStateView = base.gameState.view;
		int suppressShieldEvents = gameStateView.suppressShieldEvents + 1;
		gameStateView.suppressShieldEvents = suppressShieldEvents;
		shield.value = Math.Min(stats[StatType.ShieldRetention], shield);
		GameStateView gameStateView2 = base.gameState.view;
		suppressShieldEvents = gameStateView2.suppressShieldEvents - 1;
		gameStateView2.suppressShieldEvents = suppressShieldEvents;
		numberOfAttacks.value = stats[StatType.NumberOfAttacks];
		base.OnTurnStart();
	}

	public override bool HasStatus(StatusType status)
	{
		return statuses[status] > 0;
	}

	public virtual void OnDeath()
	{
		foreach (Ability item in appliedAbilities.GetCardsSafe())
		{
			item.Remove(new ActionContext(item.owner, item, this));
		}
		base.tapped.value = false;
		base.dead.value = true;
	}

	public void AddBaseTraits()
	{
		foreach (DataRef<AbilityData> trait in combatantData.traits)
		{
			AddTrait(trait);
		}
	}

	public PoolKeepItemListHandle<Id<Ability>> Traits()
	{
		return Pools.UseKeepItemList(traits);
	}

	public PoolKeepItemHashSetHandle<DataRef<AbilityData>> AllTraits()
	{
		PoolKeepItemHashSetHandle<DataRef<AbilityData>> poolKeepItemHashSetHandle = Pools.UseKeepItemHashSet(combatantData.traits);
		foreach (Id<Ability> item in Traits())
		{
			poolKeepItemHashSetHandle.Add(item.value.dataRef);
		}
		return poolKeepItemHashSetHandle;
	}

	public Id<Ability> AddTrait(DataRef<AbilityData> traitData)
	{
		Ability ability = new Ability(traitData, this);
		AddTrait(ability);
		return ability;
	}

	public void AddTrait(Ability trait)
	{
		if (traits.AddUnique(trait))
		{
			int num = combatantData.traits.IndexOf(trait.dataRef);
			if (num >= 0 && traits.IndexOf(trait) != num)
			{
				_SortActiveBaseTraits();
			}
			if (trait.data.type == AbilityData.Type.Trait)
			{
				trait.Activate();
			}
			else
			{
				trait.Register();
			}
			base.gameState.SignalTraitAdded(this, trait);
		}
	}

	public Id<Ability> RemoveTrait(Ability trait)
	{
		if (!traits.Remove(trait))
		{
			return trait;
		}
		base.gameState.SignalTraitBeginRemove(this, trait);
		if (trait.data.type == AbilityData.Type.Trait)
		{
			trait.Remove(new ActionContext(this, null, this));
			trait.UnregisterRefreshTargetEvents();
		}
		else
		{
			trait.Unregister();
			trait.Unapply();
		}
		base.gameState.SignalTraitRemoved(this, trait);
		return trait;
	}

	public void RemoveTraits()
	{
		foreach (Id<Ability> item in Traits())
		{
			RemoveTrait(item);
		}
	}

	public bool HasTrait(DataRef<AbilityData> trait)
	{
		if (!trait)
		{
			return traits.Count > 0;
		}
		foreach (Id<Ability> trait2 in traits)
		{
			if (ContentRef.Equal(trait, trait2.value.dataRef))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsMissingBaseTrait(bool includePermanents = true)
	{
		foreach (DataRef<AbilityData> trait in combatantData.traits)
		{
			if (!HasTrait(trait) && (includePermanents || !trait.data.permanent))
			{
				return true;
			}
		}
		return false;
	}

	public void BeginBuffApply(Ability buffAbility)
	{
		AppliedPile appliedPile = buffAbility.data.type.GetAppliedPile();
		if (appliedAbilities.Count(appliedPile) > 0)
		{
			Ability ability = appliedAbilities.GetCards(appliedPile).First();
			base.gameState.SignalBuffReplaced(buffAbility.owner, this, ability, buffAbility);
			ability.Remove(new ActionContext(buffAbility.owner, buffAbility, this));
		}
	}

	public void EndApplyBuff(Ability buffAbility)
	{
		if (!base.dead)
		{
			appliedAbilities.Transfer(buffAbility, buffAbility.data.type.GetAppliedPile());
			base.gameState.SignalBuffPlaced(buffAbility.owner, this, buffAbility);
		}
		else
		{
			buffAbility.Consume();
		}
	}

	public IEnumerator PlayDefenseMedia()
	{
		VoiceManager.Instance.Play(base.view.transform, audio.defense, interrupt: true);
		if ((bool)combatMedia.defense)
		{
			IEnumerator wait = ProjectileMediaView.Create(base.gameState.cosmeticRandom, combatMedia.defense.data, base.view, base.view, combatMedia.defense.startDataOverride, combatMedia.defense.endDataOverride, combatMedia.defense.finishedAtOverride).WaitTillFinished();
			while (wait.MoveNext())
			{
				yield return null;
			}
		}
	}

	public void HighlightStatusTrait(StatusType status, float duration = 6f)
	{
		foreach (Id<Ability> item in Traits())
		{
			foreach (AAction action in item.value.data.actions)
			{
				if (action is StatusAction statusAction && statusAction.status == status)
				{
					item.value.HighlightAbilityName(duration);
					break;
				}
			}
		}
	}

	public override string ToString()
	{
		return combatantData?.name ?? base.ToString();
	}
}
