using System;
using ProtoBuf;

[ProtoContract]
[Flags]
public enum Orients8 : byte
{
	Right = 1,
	RightUp = 2,
	Up = 4,
	LeftUp = 8,
	Left = 0x10,
	LeftDown = 0x20,
	Down = 0x40,
	RightDown = 0x80
}
