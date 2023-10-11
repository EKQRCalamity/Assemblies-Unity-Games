using ProtoBuf;

public struct CanAttackResult
{
	[ProtoContract(EnumPassthru = true)]
	public enum PreventedBy
	{
		Nothing,
		Stealth,
		Guard,
		Tapped,
		Dead,
		OutOfAttacks,
		Status,
		InvalidHand,
		CannotAttackWithHighCard,
		CannotAttackWithPair,
		CannotAttackWithTwoPair,
		CannotAttackWithThreeOfAKind,
		CannotAttackWithStraight,
		CannotAttackWithFlush,
		CannotAttackWithFullHouse,
		CannotAttackWithFourOfAKind,
		CannotAttackWithStraightFlush,
		CannotAttackWithRoyalFlush,
		CannotAttackWithFiveOfAKind,
		CannotBeAttackedWithHighCard,
		CannotBeAttackedWithPair,
		CannotBeAttackedWithTwoPair,
		CannotBeAttackedWithThreeOfAKind,
		CannotBeAttackedWithStraight,
		CannotBeAttackedWithFlush,
		CannotBeAttackedWithFullHouse,
		CannotBeAttackedWithFourOfAKind,
		CannotBeAttackedWithStraightFlush,
		CannotBeAttackedWithRoyalFlush,
		CannotBeAttackedWithFiveOfAKind,
		InvalidDefenseHandCount,
		Pacifist
	}

	public readonly ACombatant attacker;

	public readonly ACombatant defender;

	public readonly PreventedBy preventedBy;

	public bool canAttack => preventedBy == PreventedBy.Nothing;

	public ACombatant combatant => attacker ?? defender;

	public CanAttackResult(ACombatant attacker, ACombatant defender = null)
	{
		preventedBy = PreventedBy.Nothing;
		this.attacker = attacker;
		this.defender = defender;
		if (attacker != null)
		{
			if (attacker.deadOrDieing)
			{
				preventedBy = PreventedBy.Dead;
			}
			else if ((bool)attacker.tapped)
			{
				preventedBy = PreventedBy.Tapped;
			}
			else if ((int)attacker.numberOfAttacks <= 0)
			{
				preventedBy = PreventedBy.OutOfAttacks;
			}
			else if ((bool)attacker.statuses.cannotAttack)
			{
				preventedBy = PreventedBy.Status;
			}
			else if (attacker.HasStatus(StatusType.Pacifist) && attacker.gameState.EntityWithoutStatusExists(StatusType.Pacifist, attacker.faction))
			{
				preventedBy = PreventedBy.Pacifist;
			}
			if (preventedBy != 0)
			{
				return;
			}
		}
		if (defender != null && attacker != null && attacker.faction == Faction.Player)
		{
			if (defender.HasStatus(StatusType.Stealth) && defender.gameState.EntityWithoutStatusExists(StatusType.Stealth, Faction.Enemy))
			{
				preventedBy = PreventedBy.Stealth;
			}
			else if (!defender.HasStatus(StatusType.Guard) && defender.gameState.EntityWithStatusExists(StatusType.Guard, Faction.Enemy))
			{
				preventedBy = PreventedBy.Guard;
			}
		}
	}

	public CanAttackResult Message()
	{
		if (canAttack)
		{
			return this;
		}
		switch (preventedBy)
		{
		case PreventedBy.Stealth:
			defender.HighlightStatusTrait(StatusType.Stealth, 3f);
			break;
		case PreventedBy.Guard:
			foreach (AEntity entity in defender.gameState.GetEntities(defender.faction))
			{
				if (entity is ACombatant aCombatant)
				{
					aCombatant.HighlightStatusTrait(StatusType.Guard, 5f);
				}
			}
			break;
		}
		combatant.gameState.view.LogError(preventedBy.Localize(), combatant.gameState.player.audio.character.error[preventedBy, true]);
		return this;
	}

	public static implicit operator bool(CanAttackResult result)
	{
		return result.canAttack;
	}
}
