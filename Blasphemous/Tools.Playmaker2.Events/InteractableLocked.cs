using HutongGames.PlayMaker;
using Tools.Level;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an interactable is locked.")]
public class InteractableLocked : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject interactable;

	public FsmBool listenOnlySelf;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		if (listenOnlySelf.Value)
		{
			Interactable.SLocked += ListenToSelf;
		}
		else
		{
			Interactable.SLocked += ListenToAll;
		}
	}

	private void ListenToAll(Interactable go)
	{
		interactable.Value = go.gameObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	private void ListenToSelf(Interactable go)
	{
		Interactable componentInChildren = base.Owner.GetComponentInChildren<Interactable>();
		if (go.Equals(componentInChildren))
		{
			interactable.Value = go.gameObject;
			base.Fsm.Event(onSuccess);
		}
		Finish();
	}

	public override void OnExit()
	{
		if (listenOnlySelf.Value)
		{
			Interactable.SLocked -= ListenToSelf;
		}
		else
		{
			Interactable.SLocked -= ListenToAll;
		}
	}
}
