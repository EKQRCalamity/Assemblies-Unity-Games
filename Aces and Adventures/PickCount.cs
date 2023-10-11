using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PickCount
{
	Zero,
	One,
	Two,
	Three,
	Four,
	Five,
	All,
	AddAbility
}
