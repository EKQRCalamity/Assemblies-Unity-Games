using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum AudioCategoryTypeFlags
{
	Grunt = 1,
	Attack = 2,
	Hurt = 4,
	CriticallyHurt = 8,
	Death = 0x10,
	TurnStart = 0x20,
	HostileWords = 0x40,
	FriendlyWords = 0x80,
	AbilityVocal = 0x100,
	ProjectileLaunch = 0x200,
	ProjectileImpact = 0x400,
	Adventure = 0x800,
	Dialogue = 0x1000,
	Music = 0x2000,
	Ambient = 0x4000,
	Select = 0x8000,
	FindItem = 0x10000,
	BuyItem = 0x20000,
	Restore = 0x40000,
	Level = 0x80000,
	Victory = 0x100000,
	Error = 0x200000
}
