using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TopDeckResult
{
	Success,
	Failure,
	None
}
