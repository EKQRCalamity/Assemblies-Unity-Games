using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

internal class WaitUntilActionCustomCallback : CustomYieldInstruction
{
	private EnemyAction action;

	public override bool keepWaiting => !action.CallbackCalled;

	public WaitUntilActionCustomCallback(EnemyAction action)
	{
		this.action = action;
	}
}
