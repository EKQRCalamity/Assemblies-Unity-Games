public class GameStepActionActMedia : AGameStepActionMedia
{
	protected override ActionMedia media => base.action.actMedia;

	public GameStepActionActMedia(AAction action, ActionContext context)
		: base(action, context)
	{
	}
}
