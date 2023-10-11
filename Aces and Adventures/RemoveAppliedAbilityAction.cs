using ProtoBuf;

[ProtoContract]
[UIField("Remove Applied Ability", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Removes buffs and debuffs from a target combatant.", category = "Combatant")]
public class RemoveAppliedAbilityAction : ACombatantAction
{
	[ProtoMember(1)]
	[UIField(tooltip = "Bring applied abilities back into owner's hand.")]
	private bool _bounce;

	public override bool ShouldTick(ActionContext context)
	{
		if (base.ShouldTick(context))
		{
			return (context.target as ACombatant)?.appliedAbilities.Any() ?? false;
		}
		return false;
	}

	protected override bool _ShouldActUnique(ActionContext context, ACombatant combatant)
	{
		if (base._ShouldActUnique(context, combatant))
		{
			return combatant.appliedAbilities.Any();
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		foreach (Ability item in combatant.appliedAbilities.GetCardsSafe())
		{
			item.Remove(context);
			if (_bounce)
			{
				context.gameState.player.abilityDeck.Transfer(item, Ability.Pile.Hand);
			}
		}
	}

	protected override string _ToStringUnique()
	{
		return (_bounce ? "Bounce" : "Remove") + " Applied Abilities on";
	}
}
