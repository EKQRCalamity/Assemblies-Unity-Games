using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Checks if a given Game Mode is the one that is currently active.")]
public class CheckGameModeActive : FsmStateAction
{
	[RequiredField]
	public FsmString mode;

	public FsmEvent modeIsActive;

	public FsmEvent modeIsInactive;

	public override void OnEnter()
	{
		if (Core.GameModeManager.CheckGameModeActive(mode.Value))
		{
			base.Fsm.Event(modeIsActive);
		}
		else
		{
			base.Fsm.Event(modeIsInactive);
		}
		Finish();
	}
}
