using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ActionContextTarget
{
	Owner,
	Target,
	Attacker,
	Defender,
	Player,
	EnemyInActiveCombat,
	FirstEnemyInTurnOrder,
	TickTarget
}
