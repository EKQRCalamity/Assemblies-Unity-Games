using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum EnemyRank
{
	Standard,
	PlusOne,
	PlusTwo,
	PlusThree
}
