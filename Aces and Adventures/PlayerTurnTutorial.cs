using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayerTurnTutorial
{
	IdleAttack,
	IdleAbility,
	IdleEndTurn,
	ActAttack,
	ActAbility,
	ActAttackAndAbility,
	ActInvalid,
	ConfirmAttack,
	Defend,
	ConfirmDefense,
	ConfirmAdventureResults
}
