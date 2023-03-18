using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Starts an audio tool.")]
public class AudioToolStart : FsmStateAction
{
	public FsmGameObject gameObject;

	public FsmFloat fadeTime;

	public FsmFloat delay;

	public override void OnEnter()
	{
		Finish();
	}
}
