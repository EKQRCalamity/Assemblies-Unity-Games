using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileFlightLight/", false)]
public enum ProjectileFlightLightType : ushort
{
	TEMP,
	Pulsing
}
