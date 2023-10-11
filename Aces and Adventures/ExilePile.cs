using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ExilePile
{
	PlayerResource,
	EnemyResource,
	Character,
	ClearGameState
}
