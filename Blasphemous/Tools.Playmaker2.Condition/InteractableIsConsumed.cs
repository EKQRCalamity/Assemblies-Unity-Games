using HutongGames.PlayMaker;
using Tools.Level;
using UnityEngine;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
public class InteractableIsConsumed : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public GameObject interactable;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		Interactable component = interactable.GetComponent<Interactable>();
		base.Fsm.Event((!component.Consumed) ? onFailure : onSuccess);
		Finish();
	}
}
