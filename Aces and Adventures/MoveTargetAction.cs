using System;
using System.ComponentModel;
using ProtoBuf;

[ProtoContract]
[UIField("Move Target", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class MoveTargetAction : ACombatantAction
{
	private const int MIN = -10;

	private const int MAX = 10;

	[ProtoMember(1)]
	[UIField(min = -10, max = 10)]
	[DefaultValue(10)]
	private int _moveDistance = 10;

	public override bool affectsTurnOrder => true;

	private int _NewTurnOrderIndex(ActionContext context, ACombatant combatant)
	{
		return context.gameState.GetTurnOrder(combatant) + _moveDistance;
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
		return "Move";
	}

	protected override string _ToStringAfterTarget()
	{
		if (_moveDistance != -10)
		{
			if (_moveDistance != 10)
			{
				return string.Format(" {0} {1} in turn order ", Math.Abs(_moveDistance), (_moveDistance >= 0) ? "right" : "left");
			}
			return " to end of turn order ";
		}
		return " to start of turn order ";
	}
}
