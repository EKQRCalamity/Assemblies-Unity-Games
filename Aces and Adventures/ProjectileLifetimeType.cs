using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProjectileLifetimeType
{
	Impact,
	BeginFadingOut,
	FinishedFadingOut
}
