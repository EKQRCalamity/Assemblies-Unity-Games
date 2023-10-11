using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProjectileFlightOrientType : byte
{
	Velocity,
	Target,
	LaunchDirection,
	Camera,
	ImpactShape
}
