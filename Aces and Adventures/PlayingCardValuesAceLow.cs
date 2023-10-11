using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum PlayingCardValuesAceLow
{
	One = 2,
	Two = 4,
	Three = 8,
	Four = 0x10,
	Five = 0x20,
	Six = 0x40,
	Seven = 0x80,
	Eight = 0x100,
	Nine = 0x200,
	Ten = 0x400,
	Jack = 0x800,
	Queen = 0x1000,
	King = 0x2000,
	Ace = 0x4000
}
