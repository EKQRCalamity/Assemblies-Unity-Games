using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

public class PlayerGrounded : FsmStateAction
{
	public FsmBool CheckOnlyOnce;

	public FsmEvent IsGrounded;

	public FsmEvent IsNotGrounded;

	private bool alreadyTriggeredAnEvent;

	public override void OnEnter()
	{
		alreadyTriggeredAnEvent = false;
		if (Core.Logic.Penitent.Status.IsGrounded)
		{
			base.Fsm.Event(IsGrounded);
			alreadyTriggeredAnEvent = true;
		}
		else if (CheckOnlyOnce.Value)
		{
			base.Fsm.Event(IsNotGrounded);
		}
	}

	public override void OnUpdate()
	{
		if (!CheckOnlyOnce.Value && !alreadyTriggeredAnEvent && Core.Logic.Penitent.Status.IsGrounded)
		{
			base.Fsm.Event(IsGrounded);
			alreadyTriggeredAnEvent = true;
		}
	}
}
