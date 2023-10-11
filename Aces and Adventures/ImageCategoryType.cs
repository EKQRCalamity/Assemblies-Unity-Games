using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ImageCategoryType : byte
{
	Ability,
	Enemy,
	Adventure
}
