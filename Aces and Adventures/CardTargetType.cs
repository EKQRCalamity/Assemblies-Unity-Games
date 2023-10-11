using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum CardTargetType
{
	Target,
	Ability,
	AbilityOwner,
	TurnOrder,
	Player,
	EnemyCombatant,
	TriggeredBy,
	TriggeredOn
}
