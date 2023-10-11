using ProtoBuf;

[ProtoContract]
[UIField("Restore Traits", 0u, null, null, null, null, null, null, false, null, 5, false, null, category = "Combatant")]
public class RestoreTraitAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Should permanent traits be restored as well?")]
	private bool _restorePermanents;

	private bool _IsValidNegatedTrait(NegateTraitAction negateTrait)
	{
		if (!_restorePermanents)
		{
			return !negateTrait.canNegatePermanents;
		}
		return true;
	}

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		return ShouldTick(context);
	}

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context) && context.target is Enemy enemy)
		{
			return enemy.IsMissingBaseTrait(_restorePermanents);
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.ClearAppliedActionsOn<NegateTraitAction>(combatant, _IsValidNegatedTrait);
	}

	protected override string _ToStringUnique()
	{
		return "Restore All Traits" + _restorePermanents.ToText(" (including permanent)".SizeIfNotEmpty()) + " on";
	}
}
