using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum CombatPokerHandTypes
{
	CanAttackWith,
	CanBeDefendedWith,
	CanDefendWith,
	CanBeAttackedWith
}
