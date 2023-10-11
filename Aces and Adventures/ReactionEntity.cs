using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ReactionEntity
{
	Owner,
	AppliedOn,
	Enemy,
	Anyone,
	Summon,
	Ally,
	OtherAlly,
	NotOwner,
	NotAppliedOn
}
