public class GameStepActionTickMedia : AGameStepActionMedia
{
	protected override ActionMedia media => base.action.tickMedia;

	public GameStepActionTickMedia(AAction action, ActionContext context)
		: base(action, context)
	{
	}

	protected override void End()
	{
		if (_count > 0)
		{
			base.context.ability?.HighlightAbilityName();
		}
	}
}
