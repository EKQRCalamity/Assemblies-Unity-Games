using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

public class PlayerVerticalAttack : FsmStateAction
{
	public FsmBool CheckOnlyOnce;

	public FsmEvent IsVerticalAttacking;

	public FsmEvent IsNotVerticalAttacking;

	private bool alreadyTriggeredAnEvent;

	public override void OnEnter()
	{
		alreadyTriggeredAnEvent = false;
		if (Core.Logic.Penitent.VerticalAttack.Casting)
		{
			base.Fsm.Event(IsVerticalAttacking);
			alreadyTriggeredAnEvent = true;
		}
		else if (CheckOnlyOnce.Value)
		{
			base.Fsm.Event(IsNotVerticalAttacking);
		}
	}

	public override void OnUpdate()
	{
		if (!CheckOnlyOnce.Value && !alreadyTriggeredAnEvent && Core.Logic.Penitent.VerticalAttack.Casting)
		{
			base.Fsm.Event(IsVerticalAttacking);
			alreadyTriggeredAnEvent = true;
		}
	}
}
