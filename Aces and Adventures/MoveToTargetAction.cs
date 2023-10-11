using ProtoBuf;

[ProtoContract]
[UIField("Move To Target", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class MoveToTargetAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum Position
	{
		InFrontOf,
		Behind,
		ToOtherSideOf,
		NextTo
	}

	[ProtoMember(1)]
	[UIField]
	private Position _position;

	public override bool affectsTurnOrder => true;

	private int _NewTurnOrderIndex(ActionContext context, ACombatant combatant)
	{
		int turnOrder = context.gameState.GetTurnOrder(context.actor);
		int turnOrder2 = context.gameState.GetTurnOrder(combatant);
		int num = turnOrder2;
		return num + _position switch
		{
			Position.Behind => 1, 
			Position.ToOtherSideOf => (turnOrder2 - turnOrder > 0).ToInt(), 
			Position.NextTo => (turnOrder2 - turnOrder < 0).ToInt(), 
			_ => 0, 
		};
	}

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		if (base._ShouldActUnique(context, combatant))
		{
			return context.actor.alive;
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && context.actor.alive)
		{
			return context.gameState.WouldChangeTurnOrder(context.actor, _NewTurnOrderIndex(context, context.target as ACombatant));
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.SetTurnOrder(context.actor, _NewTurnOrderIndex(context, combatant));
		if (!context.gameState.adventureDeck.isSuppressingEvents)
		{
			context.gameState.stack.Append(new GameStepWaitForCardTransition(context.actor.view));
		}
	}

	protected override string _ToStringUnique()
	{
		return "Move " + EnumUtil.FriendlyName(_position);
	}
}
