using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum LevelUpPile
{
	VialDraw,
	Vial,
	VialPour,
	VialDiscard,
	Seals,
	ActiveSeal,
	PotDraw,
	Pot,
	PotDiscard,
	LeafExit,
	PresentLevelUp,
	DiscardLevelUp,
	LevelUps,
	LevelUpsView,
	LevelUpsTransition
}
