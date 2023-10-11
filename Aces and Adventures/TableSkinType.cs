using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/TableSkin/", false)]
public enum TableSkinType : ushort
{
	Default,
	Invernus
}
