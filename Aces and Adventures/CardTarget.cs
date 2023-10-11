using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum CardTarget
{
	Cost,
	Name,
	ImageCenter,
	Center,
	Description,
	Offense,
	HP,
	Defense
}
