using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum MusicPlayType
{
	Play,
	Resume,
	Stop
}
