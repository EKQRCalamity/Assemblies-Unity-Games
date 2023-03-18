using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Heals the selected entity.")]
public class EntityHeal : FsmStateAction
{
	public Entity entity;

	public FsmFloat value;

	public override void OnEnter()
	{
		if (entity != null)
		{
			entity.SetHealth(entity.CurrentLife + value.Value);
		}
	}
}
