using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TargetCountType
{
	SingleTarget,
	MultiTarget
}
