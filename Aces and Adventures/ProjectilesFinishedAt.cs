using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProjectilesFinishedAt
{
	Immediate,
	Launched,
	Impact,
	FadeOut
}
