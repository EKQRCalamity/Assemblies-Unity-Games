using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows the Penitence Selection Popup.")]
public class ShowPenitenceSelectionPopup : FsmStateAction
{
	public FsmEvent continueAfterActivatingPenitence;

	public FsmEvent continueWithoutChoosingPenitence;

	public override void OnEnter()
	{
		UIController.instance.ShowChoosePenitenceWidget(TriggerEventContinueAfterActivatingPenitence, TriggerEventContinueWithoutChoosingPenitence);
		Finish();
	}

	public void TriggerEventContinueAfterActivatingPenitence()
	{
		base.Fsm.Event(continueAfterActivatingPenitence);
	}

	public void TriggerEventContinueWithoutChoosingPenitence()
	{
		base.Fsm.Event(continueWithoutChoosingPenitence);
	}
}
