using ProtoBuf;

[ProtoContract]
[UIField("Remove Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class RemoveResourceCardAction : AResourceAction
{
	[ProtoMember(1)]
	[UIField]
	private bool _removeFromDeck;

	protected override void _Tick(ActionContext context, ResourceCard resourceCard)
	{
		if (_removeFromDeck)
		{
			context.gameState.exileDeck.layout.TransferWithSpecialTransitions(resourceCard, ExilePile.ClearGameState);
		}
		else
		{
			resourceCard.deck.layout.TransferWithSpecialTransitions(resourceCard, ResourceCard.Pile.DiscardPile);
		}
	}

	protected override string _ToStringUnique()
	{
		return "Remove";
	}

	protected override string _ToStringAfterTarget()
	{
		return _removeFromDeck.ToText(" from Deck");
	}
}
