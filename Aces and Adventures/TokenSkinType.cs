using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/TokenSkin/", false)]
public enum TokenSkinType : ushort
{
	Default,
	Invernus
}
