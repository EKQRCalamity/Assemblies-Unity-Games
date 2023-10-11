using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum EndTurnPreventedBy
{
	UsingAbility,
	PreparingAttack
}
