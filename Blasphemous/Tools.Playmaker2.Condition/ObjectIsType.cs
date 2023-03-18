using System;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using HutongGames.PlayMaker;
using Tools.Level;
using Tools.Level.Actionables;
using Tools.NPCs;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
public class ObjectIsType : FsmStateAction
{
	public FsmGameObject entity;

	public ObjectCategory type;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		if (entity.Value == null)
		{
			Finish();
			return;
		}
		switch (type)
		{
		case ObjectCategory.Destructible:
		{
			BreakableObject componentInParent2 = entity.Value.GetComponentInParent<BreakableObject>();
			base.Fsm.Event((!(componentInParent2 != null)) ? onFailure : onSuccess);
			break;
		}
		case ObjectCategory.Enemy:
		{
			Enemy componentInParent4 = entity.Value.GetComponentInParent<Enemy>();
			base.Fsm.Event((!(componentInParent4 != null)) ? onFailure : onSuccess);
			break;
		}
		case ObjectCategory.Penitent:
		{
			Penitent componentInParent3 = entity.Value.GetComponentInParent<Penitent>();
			base.Fsm.Event((!(componentInParent3 != null)) ? onFailure : onSuccess);
			break;
		}
		case ObjectCategory.NPC:
		{
			NPC componentInParent5 = entity.Value.GetComponentInParent<NPC>();
			base.Fsm.Event((!(componentInParent5 != null)) ? onFailure : onSuccess);
			break;
		}
		case ObjectCategory.Interactable:
		{
			Interactable componentInParent = entity.Value.GetComponentInParent<Interactable>();
			base.Fsm.Event((!(componentInParent != null)) ? onFailure : onSuccess);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		Finish();
	}
}
