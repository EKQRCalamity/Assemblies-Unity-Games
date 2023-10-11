using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileFlightEmitter/", false)]
public enum ProjectileFlightEmitterType : ushort
{
	FeathersBlack,
	TeleportParticles,
	TEMP,
	WaterSplashes,
	RockRubble,
	Coins,
	Leaves,
	FeathersColor,
	Gears
}
