using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum DamageSource
{
	Attack,
	Defense,
	Ability
}
