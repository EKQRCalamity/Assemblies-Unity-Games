using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Damages the selected entity.")]
public class EntityDamage : FsmStateAction
{
	public Entity entity;

	public FsmFloat value;

	public override void OnEnter()
	{
		if (entity != null)
		{
			entity.Damage(value.Value, string.Empty);
		}
	}
}
