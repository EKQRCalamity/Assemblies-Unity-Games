using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum LevelUpMessages
{
	InsufficientExperience,
	AlreadyMaxLevel,
	CardPackUnlocked,
	ManaTillCardPack,
	ManaTillLevel,
	YouCanLevelUp,
	DemoMaxLevel,
	CollectManaToUnlock,
	CompleteAdventureToUnlock,
	AvailableLevelUps
}
