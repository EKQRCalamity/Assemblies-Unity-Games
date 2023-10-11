using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum StoneType
{
	Turn,
	Cancel
}
