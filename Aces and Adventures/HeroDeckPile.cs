using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum HeroDeckPile
{
	Draw,
	SelectionHand,
	Discard
}
