using System;

[Flags]
public enum CardTargets
{
	Cost = 1,
	Name = 2,
	ImageCenter = 4,
	Center = 8,
	Description = 0x10,
	Offense = 0x20,
	HP = 0x40,
	Defense = 0x80
}
