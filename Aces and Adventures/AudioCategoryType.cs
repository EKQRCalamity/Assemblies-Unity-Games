using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AudioCategoryType : byte
{
	Grunt,
	Attack,
	Hurt,
	CriticallyHurt,
	Death,
	TurnStart,
	HostileWords,
	FriendlyWords,
	AbilityVocal,
	ProjectileLaunch,
	ProjectileImpact,
	Adventure,
	Dialogue,
	Music,
	Ambient,
	Select,
	FindItem,
	BuyItem,
	Restore,
	Level,
	Victory,
	Error
}
