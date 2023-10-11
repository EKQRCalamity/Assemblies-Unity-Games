using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum DeckPile
{
	InactiveSelectAdventure,
	InactiveSelectAbility,
	Select,
	SelectLarge,
	Adventure,
	Ability,
	AdventureOpen,
	AbilityOpen,
	Exile
}
