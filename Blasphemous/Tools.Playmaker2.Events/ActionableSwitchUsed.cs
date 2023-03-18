using HutongGames.PlayMaker;
using Tools.Level.Actionables;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an interactable is interacted.")]
public class ActionableSwitchUsed : FsmStateAction
{
	public override void OnEnter()
	{
		base.Owner.GetComponent<ActionableSwitch>().OnSwitchUsed += OnSwitchUsed;
	}

	private void OnSwitchUsed(ActionableSwitch go)
	{
		Finish();
	}

	public override void OnExit()
	{
		base.Owner.GetComponent<ActionableSwitch>().OnSwitchUsed -= OnSwitchUsed;
	}
}
