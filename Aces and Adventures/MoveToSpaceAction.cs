using ProtoBuf;

[ProtoContract]
[UIField("Move To Space", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Turn Order Space")]
public class MoveToSpaceAction : ASpaceAction
{
	public override bool affectsTurnOrder => true;

	protected override bool _ShouldActUnique(ActionContext context, TurnOrderSpace space)
	{
		if (base._ShouldActUnique(context, space))
		{
			return context.actor.alive;
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && context.actor.alive)
		{
			return context.gameState.WouldChangeTurnOrder(context.actor, ((context.target as TurnOrderSpace)?.index).GetValueOrDefault());
		}
		return false;
	}

	protected override void _Tick(ActionContext context, TurnOrderSpace space)
	{
		context.gameState.SetTurnOrder(context.actor, space.index.GetValueOrDefault());
		if (!context.gameState.adventureDeck.isSuppressingEvents)
		{
			context.gameState.stack.Append(new GameStepWaitForCardTransition(context.actor.view));
		}
	}

	protected override string _ToStringUnique()
	{
		return "Move to";
	}
}
