using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum Vector3AnimatorType : byte
{
	Wave,
	Noise
}
