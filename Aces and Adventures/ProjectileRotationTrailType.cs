using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/ProjectileRotationTrail/", false)]
public enum ProjectileRotationTrailType : ushort
{
	TEMP
}
