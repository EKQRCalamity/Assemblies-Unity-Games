using System.Linq;
using ProtoBuf;

[ProtoContract]
[ProtoInclude(14, typeof(Ability))]
[ProtoInclude(15, typeof(ACombatant))]
public abstract class AEntity : ATarget
{
	[ProtoMember(1)]
	private BBool _tapped;

	[ProtoMember(2)]
	private BBool _dead;

	[ProtoMember(3)]
	private BBool _hasTakenTurn;

	[ProtoMember(4)]
	private BBool _activeTap;

	public BBool tapped => _tapped ?? (_tapped = new BBool());

	public BBool dead => _dead ?? (_dead = new BBool());

	public BBool hasTakenTurn => _hasTakenTurn ?? (_hasTakenTurn = new BBool());

	public BBool activeTap => _activeTap ?? (_activeTap = new BBool());

	public bool alive => !dead;

	public virtual bool deadOrDieing => dead;

	public virtual bool deadOrInsuredDeath => dead;

	public abstract Faction faction { get; }

	public virtual bool canAct => canAttack;

	public virtual bool canAttack => false;

	public virtual bool canBeAttacked => false;

	public virtual bool canUntap => true;

	public AdventureCard.Pile pile => base.gameState.adventureDeck[this].GetValueOrDefault();

	public bool isTakingTurn => base.gameState.entityTakingTurn == this;

	public bool inTurnOrder => pile == AdventureCard.Pile.TurnOrder;

	public int turnOrder => base.gameState.GetTurnOrder(this);

	private bool _tappedSpecified => _tapped;

	private bool _deadSpecified => _dead;

	private bool _hasTakenTurnSpecified => _hasTakenTurn;

	private bool _activeTapSpecified => _activeTap;

	public void Tap(AEntity tappedBy = null)
	{
		if (!dead && inTurnOrder)
		{
			if (tappedBy != null)
			{
				activeTap.value = true;
			}
			tapped.value = true;
			base.gameState.SignalEntityTap(this, tapped: true, tappedBy);
		}
	}

	public void Untap()
	{
		if (canUntap)
		{
			tapped.value = false;
			activeTap.value = false;
			base.gameState.SignalEntityTap(this, tapped: false);
		}
	}

	public virtual void OnEncounterStart()
	{
	}

	public virtual void OnEncounterEnd()
	{
	}

	public virtual void OnRoundStart()
	{
		if (!activeTap)
		{
			Untap();
		}
		hasTakenTurn.value = false;
	}

	public virtual void OnTurnStart()
	{
		base.gameState.SignalTurnStart(this);
	}

	public virtual void OnTurnEnd()
	{
		if (!tapped)
		{
			Tap();
		}
		base.gameState.SignalTurnEnd(this);
		activeTap.value = false;
	}

	public Allegiance GetAllegiance(AEntity otherEntity)
	{
		if (faction != otherEntity.faction)
		{
			return Allegiance.Foe;
		}
		return Allegiance.Friend;
	}

	public bool IsEnemy(AEntity otherEntity)
	{
		return faction.IsEnemy(otherEntity.faction);
	}

	public abstract AGameStepTurn GetTurnStep();

	public virtual bool HasStatus(StatusType status)
	{
		return false;
	}

	public PoolKeepItemListHandle<AEntity> GetEntitiesToLeftOf()
	{
		return Pools.UseKeepItemList(base.gameState.turnOrderQueue.Take(base.gameState.GetTurnOrder(this)).Reverse());
	}

	public PoolKeepItemListHandle<AEntity> GetEntitiesToRightOf()
	{
		return Pools.UseKeepItemList(base.gameState.turnOrderQueue.Skip(base.gameState.GetTurnOrder(this) + 1));
	}

	public void SetLeftOfEntitiesHasTakenTurn()
	{
		if (!isTakingTurn || !inTurnOrder)
		{
			return;
		}
		foreach (AEntity item in GetEntitiesToLeftOf())
		{
			item.hasTakenTurn.value = true;
		}
	}
}
