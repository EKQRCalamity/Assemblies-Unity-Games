using System.Collections;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class CountdownFromTen_EnemyAction : EnemyAction
{
	protected override IEnumerator BaseCoroutine()
	{
		WaitSeconds_EnemyAction waitAction = new WaitSeconds_EnemyAction();
		DebugText_EnemyAction debugText = new DebugText_EnemyAction();
		for (int i = 10; i > 0; i--)
		{
			waitAction.StartAction(owner, 1f, 1f);
			yield return waitAction.waitForCompletion;
			debugText.StartAction(owner, i.ToString());
			yield return debugText.waitForCompletion;
		}
		FinishAction();
	}
}
