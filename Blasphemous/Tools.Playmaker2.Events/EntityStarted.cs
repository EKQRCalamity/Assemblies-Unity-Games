using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Events;

[ActionCategory("Blasphemous Event")]
[Tooltip("Event raised when an entity dies.")]
public class EntityStarted : FsmStateAction
{
	[UIHint(UIHint.Variable)]
	public FsmGameObject entity;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		Entity.Started += Started;
	}

	private void Started(Entity spawnedEntity)
	{
		entity.Value = spawnedEntity.gameObject;
		base.Fsm.Event(onSuccess);
		Finish();
	}

	public override void OnExit()
	{
		Entity.Started -= Started;
	}
}
