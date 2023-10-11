using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum PlayingCardSuits
{
	Club = 1,
	Diamond = 2,
	Heart = 4,
	Spade = 8
}
