using ProtoBuf;

[ProtoContract]
[UIField("Move Target To Self", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class MoveTargetToSelfAction : ACombatantAction
{
	[ProtoContract(EnumPassthru = true)]
	public enum Position
	{
		Behind,
		InFrontOf,
		[UITooltip("Preserves which side of actor target was on.")]
		To,
		ToOtherSideOf
	}

	[ProtoMember(1)]
	[UIField]
	private Position _position;

	public override bool affectsTurnOrder => true;

	private int _NewTurnOrderIndex(ActionContext context, ACombatant combatant)
	{
		int turnOrder = context.gameState.GetTurnOrder(context.actor);
		int turnOrder2 = context.gameState.GetTurnOrder(combatant);
		int num = turnOrder;
		return num + _position switch
		{
			Position.Behind => 1, 
			Position.To => (turnOrder2 - turnOrder > 0).ToInt(), 
			Position.ToOtherSideOf => (turnOrder2 - turnOrder < 0).ToInt(), 
			_ => 0, 
		};
	}

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		if (base._ShouldActUnique(context, combatant))
		{
			return combatant.alive;
		}
		return false;
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && context.target is ACombatant aCombatant && aCombatant.alive)
		{
			return context.gameState.WouldChangeTurnOrder(aCombatant, _NewTurnOrderIndex(context, aCombatant));
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.SetTurnOrder(combatant, _NewTurnOrderIndex(context, combatant));
		if (!context.gameState.adventureDeck.isSuppressingEvents)
		{
			context.gameState.stack.Append(new GameStepWaitForCardTransition(combatant.view));
		}
	}

	protected override string _ToStringUnique()
	{
		return "Pull";
	}

	protected override string _ToStringAfterTarget()
	{
		return " " + EnumUtil.FriendlyName(_position) + " You";
	}
}
