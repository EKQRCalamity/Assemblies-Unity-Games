using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ContentVisibility : byte
{
	Public = 0,
	FriendsOnly = 3,
	ByCodeOnly = 6,
	Private = 9
}
