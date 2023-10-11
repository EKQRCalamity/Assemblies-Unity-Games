public abstract class GameStepAAction : GameStep
{
	public AAction action { get; }

	public ActionContext context { get; set; }

	protected GameStepAAction(AAction action, ActionContext context)
	{
		this.action = action;
		this.context = context;
	}
}
