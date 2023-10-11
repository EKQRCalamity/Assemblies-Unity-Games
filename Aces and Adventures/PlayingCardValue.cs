using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayingCardValue
{
	Two = 2,
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
