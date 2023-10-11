using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AttackResultType
{
	Success,
	Failure,
	Tie
}
