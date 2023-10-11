using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
[UISortEnum]
[ResourceEnum("GameState/Adventure/AdventureBack/", false)]
public enum AdventureBackType : ushort
{
	Seedling,
	Cave2,
	Desert3,
	Titansladder4,
	Sea5,
	Veldt6,
	Mountain7,
	Cardrassil8,
	Niflheim9,
	DireMire10,
	Daggerstone11,
	Cathedral12,
	Invernus13,
	ProcSpring,
	ProcSummer,
	ProcWinter,
	ProcFall,
	Credit14
}
