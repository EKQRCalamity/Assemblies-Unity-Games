using System.Linq;

public class GameStepActionTick : GameStepAAction
{
	public GameStepActionTick(AAction action, ActionContext context)
		: base(action, context)
	{
	}

	public override void Start()
	{
		foreach (ATarget target in GetPreviousSteps().OfType<GameStepActionTarget>().First().targets)
		{
			ActionContext actionContext = base.context.SetTarget(target);
			base.action.Tick(actionContext);
			base.state.SignalAbilityTick(actionContext, base.action);
		}
	}
}
