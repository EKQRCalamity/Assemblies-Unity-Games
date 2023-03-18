using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Sends onSuccess event if the entity is dead, null, or is not an entity. Otherwise returns onFailure")]
public class IsEntityDead : FsmStateAction
{
	public FsmGameObject entity;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		bool flag = true;
		if (entity.Value != null)
		{
			Entity component = entity.Value.GetComponent<Entity>();
			if (component != null && !component.Dead)
			{
				flag = false;
			}
		}
		base.Fsm.Event((!flag) ? onFailure : onSuccess);
		Finish();
	}
}
