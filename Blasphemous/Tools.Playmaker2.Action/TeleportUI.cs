using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Show a Unlock skills UI")]
public class TeleportUI : FsmStateAction
{
	public FsmEvent TeleportCancelled;

	public override void OnEnter()
	{
		TeleportWidget.OnTeleportCancelled += OnTeleportCancelled;
		UIController.instance.ShowTeleportUI();
	}

	public override void OnExit()
	{
		TeleportWidget.OnTeleportCancelled -= OnTeleportCancelled;
	}

	public void OnTeleportCancelled()
	{
		base.Fsm.Event(TeleportCancelled);
		Finish();
	}
}
