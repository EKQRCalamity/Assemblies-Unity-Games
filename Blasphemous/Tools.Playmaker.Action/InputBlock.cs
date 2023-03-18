using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Allows or blocks the input from the player.")]
public class InputBlock : FsmStateAction
{
	public FsmString inputBlockName;

	public FsmBool active;

	public override void OnEnter()
	{
		Core.Input.SetBlocker(inputBlockName.Value, active.Value);
		Finish();
	}
}
