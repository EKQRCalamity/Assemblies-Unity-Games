using ProtoBuf;

[ProtoContract]
[UIField("Cancel Target", 0u, null, null, null, null, null, null, false, null, 5, false, null, tooltip = "Adds a combatant to an excluded targets list for a given ability activation, effectively rendering them immune to the ability.", category = "Combatant")]
public class CancelTargetAction : ACombatantAction
{
	public override bool ShouldTick(ActionContext context)
	{
		if (context.gameState.stack.activeStep.parentContextGroup is GameStepGroupAbilityAct gameStepGroupAbilityAct && gameStepGroupAbilityAct.ability.data.canBeResisted)
		{
			return !gameStepGroupAbilityAct.HasHashTag(context.target.ToId<ATarget>());
		}
		return false;
	}

	protected override void _Tick(ActionContext context, ACombatant combatant)
	{
		context.gameState.stack.activeStep.parentContextGroup.AddHashTag(combatant.ToId<ATarget>());
	}

	protected override string _ToStringUnique()
	{
		return "Cancel Ability Targeting";
	}
}
