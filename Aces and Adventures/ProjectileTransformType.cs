using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProjectileTransformType : byte
{
	Back,
	Emitter,
	Center,
	PointOfImpact,
	Front
}
