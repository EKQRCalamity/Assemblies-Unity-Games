using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AudioVolumeType : byte
{
	Silent,
	Quiet,
	Soft,
	Normal,
	Loud,
	Max
}
