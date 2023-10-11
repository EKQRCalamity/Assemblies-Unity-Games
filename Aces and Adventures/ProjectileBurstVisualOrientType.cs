using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProjectileBurstVisualOrientType : byte
{
	Projectile,
	Velocity,
	Shape,
	ToTarget,
	Camera,
	Card
}
