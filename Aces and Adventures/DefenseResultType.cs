using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum DefenseResultType
{
	Invalid,
	Failure,
	Tie,
	Success
}
