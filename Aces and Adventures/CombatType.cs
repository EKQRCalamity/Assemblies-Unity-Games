using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum CombatType
{
	Attack,
	Defense
}
