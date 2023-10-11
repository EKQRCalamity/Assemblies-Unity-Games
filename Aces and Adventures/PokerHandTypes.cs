using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum PokerHandTypes
{
	HighCard = 1,
	Pair = 2,
	TwoPair = 4,
	ThreeOfAKind = 8,
	Straight = 0x10,
	Flush = 0x20,
	FullHouse = 0x40,
	FourOfAKind = 0x80,
	StraightFlush = 0x100,
	RoyalFlush = 0x200,
	FiveOfAKind = 0x400
}
