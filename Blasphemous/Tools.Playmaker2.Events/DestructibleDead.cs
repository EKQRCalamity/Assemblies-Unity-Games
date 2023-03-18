using System;
using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.Level.Actionables;
using UnityEngine;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Event raised when a destructible is destroyed.")]
public class DestructibleDead : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public GameObject destructible;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		BreakableObject.OnDead = (Core.ObjectEvent)Delegate.Combine(BreakableObject.OnDead, new Core.ObjectEvent(OnDead));
	}

	private void OnDead(GameObject destroyedObject)
	{
		destructible = destroyedObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		BreakableObject.OnDead = (Core.ObjectEvent)Delegate.Combine(BreakableObject.OnDead, new Core.ObjectEvent(OnDead));
	}
}
