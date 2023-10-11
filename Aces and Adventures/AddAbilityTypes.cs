using System;
using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[Flags]
public enum AddAbilityTypes
{
	Standard = 1,
	Buff = 2,
	Debuff = 4,
	Summon = 8
}
