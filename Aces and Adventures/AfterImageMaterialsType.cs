using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Ability/Media/AfterImageMaterials/", false)]
public enum AfterImageMaterialsType : ushort
{
	Default,
	Additive
}
