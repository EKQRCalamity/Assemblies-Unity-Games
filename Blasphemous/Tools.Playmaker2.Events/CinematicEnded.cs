using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Entity")]
[Tooltip("Event raised when a destructible is destroyed.")]
public class CinematicEnded : FsmStateAction
{
	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		Core.Cinematics.CinematicStarted += CinematicStarted;
	}

	private void CinematicStarted()
	{
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		Core.Cinematics.CinematicStarted -= CinematicStarted;
	}
}
