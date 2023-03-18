using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Fades out for a given number of seconds and then fades in.")]
public class EasyFadeOutAndIn : FsmStateAction
{
	public FsmFloat TimeInFade;

	public override void OnEnter()
	{
		Core.UI.Fade.Fade(toBlack: true, 1f, 0f, delegate
		{
			OnFadeInFinished();
		});
		Finish();
	}

	private void OnFadeInFinished()
	{
		Core.UI.Fade.Fade(toBlack: false, 1f, TimeInFade.Value);
	}
}
