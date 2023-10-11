using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum CombatTypeFilter
{
	Attack,
	Defense,
	Any
}
