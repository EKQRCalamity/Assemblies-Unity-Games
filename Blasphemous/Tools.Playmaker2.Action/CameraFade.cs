using Gameplay.UI.Widgets;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
public class CameraFade : FsmStateAction
{
	public FsmBool FadeOut;

	public FsmFloat Duration;

	public FsmColor originColor;

	public FsmColor endColor;

	public override void OnEnter()
	{
		FadeWidget instance = FadeWidget.instance;
		if (!(instance == null))
		{
			FadeWidget.OnFadeShowEnd += FadeEnd;
			FadeWidget.OnFadeHidedEnd += FadeEnd;
			instance.StartEasyFade(originColor.Value, endColor.Value, Duration.Value, FadeOut.Value);
		}
	}

	private void FadeEnd()
	{
		FadeWidget.OnFadeShowEnd -= FadeEnd;
		FadeWidget.OnFadeHidedEnd -= FadeEnd;
		Finish();
	}
}
