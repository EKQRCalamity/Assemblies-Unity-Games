using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileLaunchSFX/", false)]
public enum ProjectileLaunchSFXType : ushort
{
	SharpenAxe,
	TEMP,
	ElectricLaunch,
	SwarmBurst,
	LionSeal4,
	LionSeal5,
	BullRunic,
	LaunchFlash,
	CrowCircle,
	GlitterFlareLaunch,
	EnchantressLight,
	EnchantressHeart,
	DeathPerception,
	SnakeCircle,
	ShockwaveLaunch,
	SwarmSphere,
	ArcaneBlast,
	RockLaunch,
	ResistSphere,
	PipClub,
	PipDiamond,
	PipHeart,
	PipSpade,
	QForQueen,
	BullseyePhases,
	CrowCircleRed,
	SnakeCirclePurple,
	SwarmRingLaunch,
	Wing1,
	Wing2,
	InvernusSigil,
	SwarmRingLaunchSlowColor,
	GemCircle,
	Omega
}
