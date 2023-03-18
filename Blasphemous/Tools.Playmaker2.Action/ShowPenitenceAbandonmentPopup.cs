using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows the Penitence Abandonment Popup.")]
public class ShowPenitenceAbandonmentPopup : FsmStateAction
{
	public FsmEvent continueAfterAbandoningPenitence;

	public FsmEvent continueWithoutAbandoningPenitence;

	public override void OnEnter()
	{
		UIController.instance.ShowAbandonPenitenceWidget(TriggerEventContinueAfterAbandoningPenitence, TriggerEventContinueWithoutAbandoningPenitence);
		Finish();
	}

	public void TriggerEventContinueAfterAbandoningPenitence()
	{
		base.Fsm.Event(continueAfterAbandoningPenitence);
	}

	public void TriggerEventContinueWithoutAbandoningPenitence()
	{
		base.Fsm.Event(continueWithoutAbandoningPenitence);
	}
}
