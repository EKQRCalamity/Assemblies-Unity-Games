using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Entity")]
[Tooltip("Event raised when a destructible is destroyed.")]
public class CinematicStarted : FsmStateAction
{
	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		Core.Cinematics.CinematicEnded += CinematicEnded;
	}

	private void CinematicEnded(bool cancelled)
	{
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		Core.Cinematics.CinematicEnded -= CinematicEnded;
	}
}
