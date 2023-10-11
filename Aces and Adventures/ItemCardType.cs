using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum ItemCardType
{
	[UITooltip("Equipment, Usable Items, and Consumable Items")]
	Item,
	[UITooltip("Encounter Conditions and Encounter Abilities")]
	Encounter,
	[UITooltip("Permanent Conditions placed on player mainly through events")]
	Condition
}
