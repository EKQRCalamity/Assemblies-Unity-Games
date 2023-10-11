using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum EnumFlagSetType : byte
{
	Set,
	Add,
	Subtract
}
