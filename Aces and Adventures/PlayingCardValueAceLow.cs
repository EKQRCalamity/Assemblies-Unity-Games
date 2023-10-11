using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayingCardValueAceLow
{
	One = 1,
	Two,
	Three,
	Four,
	Five,
	Six,
	Seven,
	Eight,
	Nine,
	Ten,
	Jack,
	Queen,
	King,
	Ace
}
