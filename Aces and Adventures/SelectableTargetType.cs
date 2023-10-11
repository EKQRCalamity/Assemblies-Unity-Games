using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum SelectableTargetType
{
	Target,
	Enemy,
	ResourceCard,
	EnemyResourceCard,
	Ability,
	TurnOrderSpace
}
