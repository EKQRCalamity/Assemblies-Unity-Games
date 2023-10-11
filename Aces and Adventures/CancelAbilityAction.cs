using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField(category = "Ability Card", tooltip = "Cancels an ability which is currently activating.\n<i>Only works on abilities which are in activation hand when activating.</i>")]
public class CancelAbilityAction : AAction
{
	private static readonly Target.Combatant.Self TARGET = new Target.Combatant.Self();

	private static readonly Target.ActivatingAbility TICK_TARGET = new Target.ActivatingAbility();

	public override Target target => TARGET;

	protected override Target _tickTarget => TICK_TARGET;

	protected override void _Tick(ActionContext context)
	{
		context.gameState.stack.GetSteps().OfType<GameStepAbilityActComplete>().FirstOrDefault((GameStepAbilityActComplete step) => step.ability == context.target)?.Interrupt();
	}

	protected override string _ToStringUnique()
	{
		return "Cancel Ability <size=66%>applied on</size>";
	}
}
