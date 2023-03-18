using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an entity dies.")]
public class EntityDead : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject entity;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		Entity.Death += Dead;
	}

	private void Dead(Entity deadEntity)
	{
		entity.Value = deadEntity.gameObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		Entity.Death -= Dead;
	}
}
