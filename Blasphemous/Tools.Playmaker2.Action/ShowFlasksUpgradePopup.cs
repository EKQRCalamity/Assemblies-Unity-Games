using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows the Flasks Upgrade Popup.")]
public class ShowFlasksUpgradePopup : FsmStateAction
{
	[RequiredField]
	public FsmFloat price;

	public FsmEvent upgradeFlasks;

	public FsmEvent continueWithoutUpgrading;

	public override void OnEnter()
	{
		UIController.instance.ShowUpgradeFlasksWidget(price.Value, TriggerEventUpgradeFlasks, TriggerEventContinueWithoutUpgrading);
		Finish();
	}

	public void TriggerEventUpgradeFlasks()
	{
		base.Fsm.Event(upgradeFlasks);
	}

	public void TriggerEventContinueWithoutUpgrading()
	{
		base.Fsm.Event(continueWithoutUpgrading);
	}
}
