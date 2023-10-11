using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum SpecialGameType
{
	Procedural,
	Challenge,
	Spiral
}
