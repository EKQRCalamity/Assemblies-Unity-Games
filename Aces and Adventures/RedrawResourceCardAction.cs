using ProtoBuf;

[ProtoContract]
[UIField("Redraw Resource Card", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Resource Card")]
public class RedrawResourceCardAction : AResourceAction
{
	protected override void _Tick(ActionContext context, ResourceCard resourceCard)
	{
		context.gameState.stack.Push(context.gameState.player.resourceDeck.DiscardStep(resourceCard));
		context.gameState.stack.Push(context.gameState.player.resourceDeck.DrawStep());
	}

	protected override string _ToStringUnique()
	{
		return "Redraw";
	}
}
