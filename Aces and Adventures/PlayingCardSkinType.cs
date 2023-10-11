using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/PlayingCardSkin/", false)]
public enum PlayingCardSkinType : ushort
{
	Default,
	Enemy,
	Invernus,
	Traditional,
	LuckOfThePaw
}
