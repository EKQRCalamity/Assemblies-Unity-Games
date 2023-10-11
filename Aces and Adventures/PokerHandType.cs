using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PokerHandType
{
	HighCard,
	Pair,
	TwoPair,
	ThreeOfAKind,
	Straight,
	Flush,
	FullHouse,
	FourOfAKind,
	StraightFlush,
	RoyalFlush,
	FiveOfAKind
}
