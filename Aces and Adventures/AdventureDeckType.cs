using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Adventure/AdventureDeck/", false)]
public enum AdventureDeckType : ushort
{
	Seedling,
	EbonDessert3,
	DireMire10,
	Titansladder4,
	WombOfFlame2,
	Mistcrown7,
	Veldt6,
	Niflheim9,
	Cardrassil8,
	Muspelheim11,
	SeaOfNod5,
	Invernus13,
	Cathedral12,
	ProcFall,
	ProcSpring,
	ProcSummer,
	ProcWinter,
	Credit14,
	SpiralWinter,
	SpiralFall,
	SpiralSpring,
	SpiralSummer,
	ProcInvernal,
	SpiralInvernal,
	DailyWar,
	DailySpiral,
	DailyGauntlet
}
