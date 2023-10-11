using System;

[Flags]
public enum TargetLineTags
{
	Target = 1,
	Target2 = 2,
	Target3 = 4,
	Target4 = 8,
	Target5 = 0x10,
	Target6 = 0x20,
	Target7 = 0x40,
	Target8 = 0x80,
	Target9 = 0x100,
	Target10 = 0x200,
	Attack = 0x400,
	Defend = 0x800,
	TopDeck = 0x1000,
	Persistent = 0x2000
}
