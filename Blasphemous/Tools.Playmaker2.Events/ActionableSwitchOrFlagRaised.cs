using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.Level.Actionables;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when a flag has been set to true or an interactable is interacted.")]
public class ActionableSwitchOrFlagRaised : FsmStateAction
{
	public FsmString category;

	public FsmString flagName;

	public bool runtimeFlag;

	public FsmEvent onFlagChanged;

	public FsmEvent onSwitchUsed;

	public override void OnEnter()
	{
		Core.Events.OnFlagChanged += OnFlagChanged;
		base.Owner.GetComponent<ActionableSwitch>().OnSwitchUsed += OnSwitchUsed;
	}

	private void OnFlagChanged(string flag, bool flagactive)
	{
		if (flagName.Value == flag && flagactive)
		{
			base.Fsm.Event(onFlagChanged);
			Finish();
		}
	}

	private void OnSwitchUsed(ActionableSwitch go)
	{
		base.Fsm.Event(onSwitchUsed);
		Finish();
	}

	public override void OnExit()
	{
		Core.Events.OnFlagChanged += OnFlagChanged;
		base.Owner.GetComponent<ActionableSwitch>().OnSwitchUsed -= OnSwitchUsed;
	}
}
