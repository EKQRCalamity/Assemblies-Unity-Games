using System.Linq;

public class GameStepActionAct : GameStepAAction
{
	public GameStepActionAct(AAction action, ActionContext context)
		: base(action, context)
	{
	}

	public override void Start()
	{
		foreach (ATarget target in GetPreviousSteps().OfType<GameStepActionTarget>().First().targets)
		{
			base.action.Act(base.context.SetTarget(target));
		}
	}
}
