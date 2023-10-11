using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum RewardPile
{
	Draw,
	Select,
	CardPackSelect,
	Results,
	Discard
}
