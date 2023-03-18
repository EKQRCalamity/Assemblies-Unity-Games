using HutongGames.PlayMaker;

namespace Tools.Playmaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Stops an audio tool.")]
public class AudioToolStop : FsmStateAction
{
	public FsmGameObject gameObject;

	public FsmFloat fadeTime;

	public FsmFloat delay;

	public override void OnEnter()
	{
		Finish();
	}
}
