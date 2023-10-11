using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ProceduralPhaseType
{
	Start,
	Middle,
	End
}
