using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AdventureTutorial
{
	SelectAdventure,
	SelectCharacter,
	SelectDeck,
	NoValidDeckFound,
	CharacterLockedForDemo,
	AdventureLockedForDemo,
	SelectTrait,
	GameLockedForDemo
}
