using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum NumberOfAttacks
{
	Zero,
	One,
	Two,
	Three,
	Four,
	Five
}
