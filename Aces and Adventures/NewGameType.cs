using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum NewGameType
{
	Spring,
	Summer,
	Fall,
	Winter
}
