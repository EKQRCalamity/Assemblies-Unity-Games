using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Kills the selected entity.")]
public class EntityKill : FsmStateAction
{
	public Entity entity;

	public override void OnEnter()
	{
		if (entity != null)
		{
			entity.Kill();
		}
	}
}
