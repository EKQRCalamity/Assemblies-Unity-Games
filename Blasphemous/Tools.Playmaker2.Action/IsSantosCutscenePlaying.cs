using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Check if the Santos cutscene is playing.")]
public class IsSantosCutscenePlaying : FsmStateAction
{
	public FsmEvent yes;

	public FsmEvent no;

	public override void OnEnter()
	{
		if (Core.Cinematics.InSantosCutscene)
		{
			base.Fsm.Event(yes);
		}
		else
		{
			base.Fsm.Event(no);
		}
		Finish();
	}
}
