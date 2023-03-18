using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows the Quote Popup.")]
public class ShowQuotePopup : FsmStateAction
{
	[RequiredField]
	public FsmFloat fadeInTime;

	[RequiredField]
	public FsmFloat timeActive;

	[RequiredField]
	public FsmFloat fadeOutTime;

	public FsmEvent continueAfterFadeOut;

	public override void OnEnter()
	{
		UIController.instance.ShowQuoteWidget(fadeInTime.Value, timeActive.Value, fadeOutTime.Value, TriggerEventContinueAfterFadeOut);
		Finish();
	}

	public void TriggerEventContinueAfterFadeOut()
	{
		base.Fsm.Event(continueAfterFadeOut);
	}
}
