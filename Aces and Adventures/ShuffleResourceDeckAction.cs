using ProtoBuf;

[ProtoContract]
[UIField("Shuffle Resource Deck", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Player")]
public class ShuffleResourceDeckAction : APlayerAction
{
	protected override void _Tick(ActionContext context, Player player)
	{
		context.gameState.stack.Push(player.resourceDeck.TransferPileStep(ResourceCard.Pile.DrawPile, ResourceCard.Pile.DiscardPile));
		context.gameState.stack.Push(player.resourceDeck.ShuffleStep(ResourceCard.Pile.DiscardPile, ResourceCard.Pile.DrawPile));
	}

	protected override string _ToStringUnique()
	{
		return "Shuffle Resource Deck for";
	}
}
