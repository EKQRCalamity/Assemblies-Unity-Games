using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

public class ActiveCombat
{
	[ProtoContract(EnumPassthru = true)]
	public enum Phase
	{
		Undecided,
		Decided
	}

	[ProtoContract(EnumPassthru = true)]
	public enum DamageDataType
	{
		TotalDamage,
		TotalDamageIgnoreZeroMultiplier,
		Damage,
		DamageMultiplier,
		DamageDenominator,
		PotentialAttackDamage,
		PotentialDefenseDamage
	}

	[Flags]
	public enum Flags
	{
		PlayerAttackWildedAfterLaunch = 1,
		EnemyDefenseWildedAfterLaunch = 2
	}

	public readonly struct DamageData : IEquatable<DamageData>
	{
		public static readonly DamageData Default = new ActiveCombat(null, null).damageData;

		public readonly int damage;

		public readonly int damageMultiplier;

		public readonly int damageDenominator;

		public readonly bool zeroDamage;

		public DamageData(int damage, int damageMultiplier, int damageDenominator, bool zeroDamage)
		{
			this.damage = damage;
			this.damageMultiplier = damageMultiplier;
			this.damageDenominator = damageDenominator;
			this.zeroDamage = zeroDamage;
		}

		public static bool operator ==(DamageData a, DamageData b)
		{
			if (a.damage == b.damage && a.damageMultiplier == b.damageMultiplier && a.damageDenominator == b.damageDenominator)
			{
				return a.zeroDamage == b.zeroDamage;
			}
			return false;
		}

		public static bool operator !=(DamageData a, DamageData b)
		{
			return !(a == b);
		}

		public bool Equals(DamageData other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is DamageData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return damage | (damageMultiplier << 8) | (damageDenominator << 16) | ((zeroDamage ? 1 : 0) << 24);
		}
	}

	private bool _canceled;

	private Flags _flags;

	private AttackResultType? _resultOverride;

	public ACombatant attacker { get; }

	public ACombatant defender { get; }

	public ACombatant enemyCombatant
	{
		get
		{
			if (attacker.faction != Faction.Enemy)
			{
				return defender;
			}
			return attacker;
		}
	}

	public int damage { get; set; }

	public int damageMultiplier { get; set; } = 1;


	public int damageDenominator { get; set; } = 1;


	public bool zeroDamage { get; set; }

	public AttackResultType? result { get; set; }

	public AttackResultType? resultOverride
	{
		get
		{
			return _resultOverride;
		}
		set
		{
			AttackResultType? attackResultType2 = (result = value);
			_resultOverride = attackResultType2;
		}
	}

	public bool attackHasBeenLaunched { get; set; }

	public bool defenseHasBeenLaunched { get; set; }

	public bool resultIsFinal { get; set; }

	public bool damageHasBeenProcessed { get; set; }

	public bool canceled
	{
		get
		{
			return _canceled;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _canceled, value))
			{
				resultIsFinal |= value;
			}
		}
	}

	public AAction action { get; set; }

	public Ability ability { get; set; }

	public ACombatant victor
	{
		get
		{
			if (result == AttackResultType.Failure)
			{
				return defender;
			}
			return attacker;
		}
	}

	public ACombatant loser => GetOpponent(victor);

	public Phase phase
	{
		get
		{
			if (!resultIsFinal)
			{
				return Phase.Undecided;
			}
			return Phase.Decided;
		}
	}

	public DamageData damageData
	{
		get
		{
			return new DamageData(damage, damageMultiplier, damageDenominator, zeroDamage);
		}
		private set
		{
			damage = value.damage;
			damageMultiplier = value.damageMultiplier;
			damageDenominator = value.damageDenominator;
			zeroDamage = value.zeroDamage;
		}
	}

	public bool playerAttackWildedAfterLaunch
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.PlayerAttackWildedAfterLaunch);
		}
		set
		{
			EnumUtil.SetFlag(ref _flags, Flags.PlayerAttackWildedAfterLaunch, value);
		}
	}

	public bool enemyDefenseWildedAfterLaunch
	{
		get
		{
			return EnumUtil.HasFlag(_flags, Flags.EnemyDefenseWildedAfterLaunch);
		}
		set
		{
			EnumUtil.SetFlag(ref _flags, Flags.EnemyDefenseWildedAfterLaunch, value);
		}
	}

	private int _nonZeroDamage => Math.Max(0, GameState.DivideDamage(damage * damageMultiplier, damageDenominator));

	public int totalDamage
	{
		get
		{
			if (!zeroDamage)
			{
				return _nonZeroDamage;
			}
			return 0;
		}
	}

	public int potentialAttackDamage => _GetPotentialAttackDamage(suppressReactions: true);

	public int potentialDefenseDamage => _GetPotentialDefenseDamage(suppressReactions: true);

	public ActionContext context => new ActionContext(attacker, null, defender);

	public bool shouldBeCanceled
	{
		get
		{
			if (!canceled && !attacker.deadOrDieing)
			{
				return defender.deadOrDieing;
			}
			return true;
		}
	}

	public bool createdByAction => action != null;

	public int this[DamageDataType type] => type switch
	{
		DamageDataType.TotalDamage => totalDamage, 
		DamageDataType.TotalDamageIgnoreZeroMultiplier => _nonZeroDamage, 
		DamageDataType.Damage => damage, 
		DamageDataType.DamageMultiplier => damageMultiplier, 
		DamageDataType.DamageDenominator => damageDenominator, 
		DamageDataType.PotentialAttackDamage => _GetPotentialAttackDamage(), 
		DamageDataType.PotentialDefenseDamage => _GetPotentialDefenseDamage(), 
		_ => totalDamage, 
	};

	public static int? GetPotentialAttackDamage(ACombatant attacker, ACombatant defender, IEnumerable<ResourceCard> attackCards)
	{
		if (attacker == null || defender == null || attackCards == null)
		{
			return null;
		}
		using PoolKeepItemDictionaryHandle<ResourceCard, ResourceCard.Pile> poolKeepItemDictionaryHandle = Pools.UseKeepItemDictionary<ResourceCard, ResourceCard.Pile>();
		foreach (ResourceCard attackCard in attackCards)
		{
			poolKeepItemDictionaryHandle[attackCard] = attackCard.pile;
		}
		(PoolKeepItemListHandle<ResourceCard>, PokerHandType) attackCombatHand = attacker.GetAttackCombatHand(poolKeepItemDictionaryHandle.value.Keys, defender);
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = attackCombatHand.Item1;
		if (!poolKeepItemListHandle || poolKeepItemListHandle.Count != poolKeepItemDictionaryHandle.Count)
		{
			return null;
		}
		GameState gameState = attacker.gameState;
		ActiveCombat activeCombat = gameState.activeCombat;
		ActiveCombat activeCombat3 = (gameState.activeCombat = new ActiveCombat(attacker, defender));
		ActiveCombat activeCombat4 = activeCombat3;
		IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck = attacker.resourceDeck;
		int suppressEvents = resourceDeck.suppressEvents + 1;
		resourceDeck.suppressEvents = suppressEvents;
		int value;
		using (ResourceCard.WildSnapshot.Create(poolKeepItemListHandle.value))
		{
			foreach (ResourceCard item in poolKeepItemListHandle.value)
			{
				attacker.resourceDeck.Transfer(item, ResourceCard.Pile.AttackHand);
			}
			attackCombatHand.WildIntoPokerHand(disposeHand: false);
			value = activeCombat4.potentialAttackDamage;
		}
		foreach (ResourceCard item2 in poolKeepItemListHandle.value)
		{
			attacker.resourceDeck.Transfer(item2, poolKeepItemDictionaryHandle[item2]);
		}
		IdDeck<ResourceCard.Pile, ResourceCard> resourceDeck2 = attacker.resourceDeck;
		suppressEvents = resourceDeck2.suppressEvents - 1;
		resourceDeck2.suppressEvents = suppressEvents;
		gameState.activeCombat = activeCombat;
		return value;
	}

	public ActiveCombat(ACombatant attacker, ACombatant defender)
	{
		this.attacker = attacker;
		this.defender = defender;
	}

	private int _GetPotentialAttackDamage(bool suppressReactions = false)
	{
		if (attacker == null)
		{
			return 0;
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = GetCombatHand(attacker).hand;
		if (action == null && (!poolKeepItemListHandle || poolKeepItemListHandle.Count == 0))
		{
			return 0;
		}
		AttackResultType? attackResultType = result;
		DamageData damageData = this.damageData;
		this.damageData = DamageData.Default;
		damage = ((action == null) ? poolKeepItemListHandle.Count : damageData.damage);
		result = AttackResultType.Success;
		if (suppressReactions)
		{
			GameState gameState = attacker.gameState;
			int suppressProcessDamageReactions = gameState.suppressProcessDamageReactions + 1;
			gameState.suppressProcessDamageReactions = suppressProcessDamageReactions;
		}
		attacker.gameState.SignalProcessCombatDamage();
		if (suppressReactions)
		{
			GameState gameState2 = attacker.gameState;
			int suppressProcessDamageReactions = gameState2.suppressProcessDamageReactions - 1;
			gameState2.suppressProcessDamageReactions = suppressProcessDamageReactions;
		}
		int num = totalDamage;
		result = attackResultType;
		this.damageData = damageData;
		return num;
	}

	private int _GetPotentialDefenseDamage(bool suppressReactions = false)
	{
		if (defender == null)
		{
			return 0;
		}
		using PoolKeepItemListHandle<ResourceCard> poolKeepItemListHandle = GetCombatHand(defender).hand;
		if (!poolKeepItemListHandle || poolKeepItemListHandle.Count == 0)
		{
			return 0;
		}
		AttackResultType? attackResultType = result;
		DamageData damageData = this.damageData;
		this.damageData = DamageData.Default;
		damage = poolKeepItemListHandle.Count;
		result = AttackResultType.Failure;
		if (suppressReactions)
		{
			GameState gameState = defender.gameState;
			int suppressProcessDamageReactions = gameState.suppressProcessDamageReactions + 1;
			gameState.suppressProcessDamageReactions = suppressProcessDamageReactions;
		}
		defender.gameState.SignalProcessCombatDamage();
		if (suppressReactions)
		{
			GameState gameState2 = defender.gameState;
			int suppressProcessDamageReactions = gameState2.suppressProcessDamageReactions - 1;
			gameState2.suppressProcessDamageReactions = suppressProcessDamageReactions;
		}
		int num = totalDamage;
		result = attackResultType;
		this.damageData = damageData;
		return num;
	}

	private void _OnResourceTransfer(ResourceCard card, ResourceCard.Pile? oldPile, ResourceCard.Pile? newPile)
	{
		if ((oldPile.HasValue && oldPile.GetValueOrDefault().IsCombat()) || (newPile.HasValue && newPile.GetValueOrDefault().IsCombat()))
		{
			_UpdatePotentialCombatDamage();
		}
	}

	private void _OnWildValueChange(ResourceCard card, ResourceCard.WildContext wildContext)
	{
		if (card.pile.IsCombat())
		{
			_UpdatePotentialCombatDamage();
		}
	}

	private void _UpdatePotentialCombatDamage()
	{
		switch (damageHasBeenProcessed ? AttackResultType.Tie : GetPotentialAttackResult())
		{
		case AttackResultType.Failure:
			attacker?.combatantCard.ShowPotentialDamage(this, (!attacker.statuses.safeAttack) ? potentialDefenseDamage : 0, 0, GetAttackerSafeAttackTraits());
			defender?.combatantCard.HidePotentialDamage(this);
			break;
		case AttackResultType.Tie:
			_HidePotentialCombatDamage();
			break;
		default:
			defender?.combatantCard.ShowPotentialDamage(this, potentialAttackDamage);
			attacker?.combatantCard.HidePotentialDamage(this);
			break;
		}
	}

	private void _HidePotentialCombatDamage()
	{
		attacker?.combatantCard.HidePotentialDamage(this);
		defender?.combatantCard.HidePotentialDamage(this);
	}

	public bool IsInCombat(ACombatant combatant)
	{
		if (combatant != attacker)
		{
			return combatant == defender;
		}
		return true;
	}

	public ACombatant GetOpponent(ACombatant combatant)
	{
		if (combatant != attacker)
		{
			if (combatant != defender)
			{
				return null;
			}
			return attacker;
		}
		return defender;
	}

	public CombatType? GetCombatType(ACombatant combatant)
	{
		if (combatant != attacker)
		{
			if (combatant != defender)
			{
				return null;
			}
			return CombatType.Defense;
		}
		return CombatType.Attack;
	}

	public AttackResultType? GetResultFor(ACombatant combatant)
	{
		if (combatant != attacker)
		{
			if (combatant != defender)
			{
				return null;
			}
			return result.Opposite();
		}
		return result;
	}

	public ResourceCard.Pile GetPile(ACombatant combatant)
	{
		if (combatant != attacker)
		{
			return ResourceCard.Pile.DefenseHand;
		}
		return ResourceCard.Pile.AttackHand;
	}

	public int GetEffectiveOffense()
	{
		return attacker.GetOffenseAgainst(defender);
	}

	public int GetEffectiveDefense()
	{
		return defender.GetDefenseAgainst(attacker);
	}

	public (PoolKeepItemListHandle<ResourceCard> hand, PokerHandType handType) GetCombatHand(ACombatant combatant)
	{
		return GetCombatType(combatant) switch
		{
			CombatType.Attack => attacker.GetAttackCombatHand(ResourceCard.Pile.AttackHand, defender, attacker.resourceDeck.GetCards(ResourceCard.Pile.AttackHand)), 
			CombatType.Defense => defender.GetDefenseCombatHand(ResourceCard.Pile.DefenseHand, attacker, attacker.GetAttackHand(defender), defender.resourceDeck.GetCards(ResourceCard.Pile.DefenseHand)), 
			_ => default((PoolKeepItemListHandle<ResourceCard>, PokerHandType)), 
		};
	}

	public AttackResultType GetPotentialAttackResult()
	{
		AttackResultType attackResultType = (resultIsFinal ? result : attacker?.GetAttackHand(defender).GetAttackResultType(defender?.GetDefenseHand(attacker))) ?? AttackResultType.Tie;
		if (attackResultType == AttackResultType.Tie)
		{
			switch (defender?.GetDefenseTieBreaker(attacker))
			{
			case 1:
				attackResultType = AttackResultType.Failure;
				break;
			case -1:
				attackResultType = AttackResultType.Success;
				break;
			}
		}
		return attackResultType;
	}

	public void BeginShowPotentialDamage(GameState state)
	{
		state.playerResourceDeck.onTransfer += _OnResourceTransfer;
		state.enemyResourceDeck.onTransfer += _OnResourceTransfer;
		state.onWildValueChanged += _OnWildValueChange;
		_UpdatePotentialCombatDamage();
	}

	public void EndShowPotentialDamage(GameState state)
	{
		state.playerResourceDeck.onTransfer -= _OnResourceTransfer;
		state.enemyResourceDeck.onTransfer -= _OnResourceTransfer;
		state.onWildValueChanged -= _OnWildValueChange;
		_HidePotentialCombatDamage();
	}

	public IEnumerable<Ability> GetAttackerSafeAttackTraits()
	{
		if (!(attacker?.statuses.safeAttack))
		{
			return null;
		}
		return from id in attacker.Traits().AsEnumerable()
			where id.value.data.actions.Any((AAction a) => a is StatusAction statusAction && statusAction.status == StatusType.SafeAttack)
			select id.value;
	}
}
