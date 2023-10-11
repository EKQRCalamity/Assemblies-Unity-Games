using System;

[Flags]
[UISortEnum]
public enum LayerMaskTypes
{
	Default = 1,
	TransparentFX = 2,
	IgnoreRaycast = 4,
	Water = 0x10,
	UI = 0x20,
	Tile = 0x100,
	Wall = 0x200,
	Table = 0x400,
	WorldSpaceUI = 0x100000,
	UIOverlay = 0x200000,
	AfterImages = 0x40000000,
	Processing = int.MinValue
}
