using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AdventureResultType
{
	Experience,
	Time,
	StrategicTime,
	Continue,
	Replay,
	Retry,
	ResultDescription,
	ResultTitle,
	BonusExperience,
	NextRankTime
}
