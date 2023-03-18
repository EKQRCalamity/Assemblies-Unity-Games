using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

internal class WaitUntilActionFinishes : CustomYieldInstruction
{
	private EnemyAction action;

	public override bool keepWaiting => !action.Finished;

	public WaitUntilActionFinishes(EnemyAction action)
	{
		this.action = action;
	}
}
