using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileFlightVFX/", false)]
public enum ProjectileFlightVFXType : ushort
{
	FlamesClassic1,
	Electricity1,
	Smoke,
	SwarmBasic,
	FlamesVectory,
	Electricity3,
	FlamesClassic2,
	Electricity2,
	SpiralingCones,
	Energy,
	Distortion,
	SwirlingSpiralAir,
	GlitteringTrails,
	Swishes,
	AirCone,
	Mist,
	WaterSplashes,
	Frost,
	Rings,
	Dust,
	Wavey,
	AxeParticles,
	BubblesPopping,
	Crosses,
	FlaresGlittering,
	Distort,
	DarkParticles,
	CardPipSpade,
	CardPipClub,
	CardPipDiamond,
	CardPipHeart,
	Runes,
	FlareTrail,
	Snowflake,
	MistNonLit,
	SwarmDark,
	MinusSigns,
	Magic,
	SmokeMagic,
	Bubbles,
	SwordParticles,
	ShieldParticles,
	Splats,
	RingsBrass,
	Bats,
	SwarmNoDepth,
	SwirlingSpiralAirNoDepth
}
