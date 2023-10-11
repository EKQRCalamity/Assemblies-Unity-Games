using ProtoBuf;

[ProtoContract]
[UIField("Shuffle Ability Deck", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class ShuffleAbilityDeckAction : APlayerAction
{
	protected override void _Tick(ActionContext context, Player player)
	{
		context.gameState.stack.Push(player.abilityDeck.TransferPileStep(Ability.Pile.Draw, Ability.Pile.Discard));
		context.gameState.stack.Push(player.abilityDeck.ShuffleStep(Ability.Pile.Discard, Ability.Pile.Draw));
	}

	protected override string _ToStringUnique()
	{
		return "Shuffle Ability Deck for";
	}
}
