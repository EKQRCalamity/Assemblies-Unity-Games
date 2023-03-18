using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Deprecated")]
[Tooltip("Starts/Ends the cinematic mode.")]
public class Fade : FsmStateAction
{
	public FsmBool showing = false;

	public FsmFloat duration = 1f;

	public override void OnEnter()
	{
		Core.UI.Fade.Fade(showing.Value, duration.Value);
		Finish();
	}
}
