using ProtoBuf;

[ProtoContract]
[UIField("Set Combat Result", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class SetCombatResultAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField]
	private AttackResultType _result;

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		if (context.gameState.activeCombat != null)
		{
			return context.gameState.activeCombat.IsInCombat(combatant);
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.activeCombat.resultOverride = ((context.gameState.activeCombat.attacker == combatant) ? _result : _result.Opposite());
	}

	protected override string _ToStringUnique()
	{
		return "Set Combat Result to <b>" + EnumUtil.FriendlyName(_result) + "</b> for";
	}
}
