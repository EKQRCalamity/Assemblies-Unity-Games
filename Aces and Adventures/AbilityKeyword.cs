using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum AbilityKeyword
{
	Tap,
	AttackDamage,
	PlayerOffense,
	PlayerDefense,
	EnemyOffense,
	EnemyDefense,
	AbilityTagReaction,
	AbilityTagAttack,
	AbilityTagDefense,
	AbilityTagCombat,
	AbilityTagAsync,
	AbilityTagOnly,
	AbilityTagHeroAbility,
	AbilityTagLevel1Trait,
	AbilityTagLevel2Trait,
	AbilityTagLevel3Trait,
	AbilityTagTrumpCard,
	AbilityTagBuff,
	AbilityTagDebuff,
	AbilityTagSummon,
	AbilityTagPassive,
	AbilityTagTrait,
	AbilityTagEquipment,
	AbilityTagConsumable,
	AbilityTagUsable,
	AbilityTagItem,
	AbilityTagEncounterCondition,
	AbilityTagEncounterAbility,
	TopDeck,
	TopDeckAgainst,
	WildSuit,
	WildRange,
	Wild,
	Shields,
	IsTapped,
	StandardAttack,
	NegateTrait,
	CanReduceDefenseToZero,
	Overkill,
	PreventHealing,
	Upgrade,
	UpgradePlus,
	AttackDamageModifier,
	DefenseDamageModifier,
	CombatDamageModifier,
	StandardCombat,
	Overdraw,
	Redraw,
	AbilityDamage,
	AbilityTagCondition,
	HalfHealth,
	SafeAttack
}
