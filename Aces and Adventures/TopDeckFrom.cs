using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum TopDeckFrom
{
	PlayerDeck,
	EnemyDeck
}
