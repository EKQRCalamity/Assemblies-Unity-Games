using ProtoBuf;

[ProtoContract]
[UIField("Cancel Combat", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class CancelCombatAction : ACombatantAction
{
	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		ActiveCombat activeCombat = context.gameState.activeCombat;
		if (activeCombat != null && !activeCombat.canceled)
		{
			return activeCombat.IsInCombat(combatant);
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.activeCombat.canceled = true;
	}

	protected override string _ToStringUnique()
	{
		return "Cancel Combat Involving";
	}
}
