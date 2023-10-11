using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileFlightTrail/", false)]
public enum ProjectileFlightTrailType : ushort
{
	Basic,
	Vectory1,
	Lightning,
	Fireballish,
	AirStreak,
	Bandage,
	Chain,
	Rope,
	BasicDark,
	BasicNoDepth,
	Rainbow
}
