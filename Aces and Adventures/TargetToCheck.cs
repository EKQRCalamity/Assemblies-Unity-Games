using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TargetToCheck
{
	Active,
	Main,
	Tick
}
