using ProtoBuf;

[ProtoContract(EnumPassthru = true)]
public enum PlayerStatType
{
	[UITooltip("Determines the maximum number of resource cards that can be held in hand.")]
	ResourceHandSize,
	[UITooltip("Determines the maximum number of ability cards that can be held in hand.")]
	AbilityHandSize,
	[UITooltip("Determines how many ability cards are drawn at the start of your turn.")]
	AbilityDrawCount,
	[UITooltip("Determines how many times you can activate Hero Ability on your turn.")]
	NumberOfHeroAbilities,
	[UITooltip("Determines how many cards from the top of the player's playing card draw pile can be seen.")]
	ScryPlayingCards,
	[UITooltip("Determines how many cards from the top of the player's ability card draw pile can be seen.")]
	ScryAbilityCards,
	[UITooltip("Determines how many cards from the top of the enemy's playing card draw pile can be seen.")]
	ScryEnemyPlayingCards
}
