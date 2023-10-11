using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Procedural/MapMaterial/", false)]
public enum MapMaterialType : ushort
{
	Procedural,
	Invernal,
	InvernalDark,
	ProceduralDark
}
