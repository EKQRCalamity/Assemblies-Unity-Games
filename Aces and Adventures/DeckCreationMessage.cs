using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum DeckCreationMessage
{
	NormalCardMax,
	EliteCardMax,
	LegendaryCardMax,
	DeckIsFull,
	MaxCopiesOfCardInDeck,
	NoMoreCopiesOfCardFound,
	SelectCharacterTitle,
	SelectDeckTitle,
	EditDeckTitle,
	NewDeck,
	Page,
	Back,
	Exit,
	Done,
	DestroyDeck,
	Cancel,
	UnnamedDeck,
	DestroyDeckMessage,
	MaxDecksAlreadyExist,
	IncompleteDeck,
	AbilityNotUnlocked
}
