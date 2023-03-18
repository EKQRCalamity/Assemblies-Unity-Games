using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an entity is attacked.")]
public class EntityAttacked : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject entity;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		Entity.Damaged += Damaged;
	}

	private void Damaged(Entity damagedEntity)
	{
		entity.Value = damagedEntity.gameObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		Entity.Damaged -= Damaged;
	}
}
