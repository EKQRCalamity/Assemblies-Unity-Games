using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum StatType
{
	Offense,
	Defense,
	Health,
	NumberOfAttacks,
	ShieldRetention
}
