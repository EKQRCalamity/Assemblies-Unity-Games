using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum StatusType
{
	CannotAttack,
	CannotUntap,
	CanBeReducedToZeroDefense,
	RedrawAttack,
	Guard,
	Stealth,
	SafeAttack,
	AbilityGuard,
	AbilityStealth,
	Pacifist,
	CanReduceEnemyDefenseToZero
}
