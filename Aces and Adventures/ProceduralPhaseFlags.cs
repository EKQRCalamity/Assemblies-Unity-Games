using System;
using ProtoBuf;

[Flags]
[ProtoContract(EnumPassthru = true)]
public enum ProceduralPhaseFlags
{
	Start = 1,
	Middle = 2,
	End = 4
}
