using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TargetOfReaction
{
	TriggeredBy,
	TriggeredOn,
	Owner,
	Anyone,
	Summon,
	Player,
	Ability,
	AppliedOn
}
