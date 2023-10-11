using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum LeaderboardType
{
	None,
	Daily,
	Permanent
}
