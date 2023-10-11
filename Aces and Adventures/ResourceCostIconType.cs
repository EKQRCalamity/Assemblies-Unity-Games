using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ResourceCostIconType
{
	Club,
	Diamond,
	Heart,
	Spade,
	Black,
	Red,
	AnyCard,
	Value,
	ValueOrHigher,
	ValueOrLower,
	ValueAndSuit,
	ValueOrHigherAndSuit,
	ValueOrLowerAndSuit,
	FaceCard,
	Flexible,
	Attack,
	Shield,
	HP,
	PokerHand,
	FaceCardAndSuit
}
