using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayingCardSuit
{
	Club,
	Diamond,
	Heart,
	Spade
}
