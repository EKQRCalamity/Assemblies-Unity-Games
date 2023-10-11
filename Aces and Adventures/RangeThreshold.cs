using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum RangeThreshold
{
	Within,
	OutsideOf
}
