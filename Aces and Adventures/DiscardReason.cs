using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum DiscardReason
{
	HandFull,
	PayingForItem,
	EnemyEffect,
	PlayerEffect,
	Mulligan
}
