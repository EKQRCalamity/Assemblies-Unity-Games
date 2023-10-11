using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TickTargetType
{
	Default,
	Custom,
	UseActTarget
}
