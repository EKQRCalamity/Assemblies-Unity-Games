using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum MatureContentFlags : byte
{
	MatureThemes = 1,
	Violence = 2,
	Blood = 4,
	Gore = 8,
	Language = 0x10,
	Drugs = 0x20,
	SexualContent = 0x40,
	Nudity = 0x80
}
