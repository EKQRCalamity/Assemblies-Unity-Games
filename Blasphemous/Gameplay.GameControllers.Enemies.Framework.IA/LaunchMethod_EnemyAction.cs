using System;
using System.Collections;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class LaunchMethod_EnemyAction : EnemyAction
{
	private Action method;

	public EnemyAction StartAction(EnemyBehaviour e, Action _method)
	{
		method = _method;
		return StartAction(e);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
		method();
	}

	protected override IEnumerator BaseCoroutine()
	{
		yield return null;
		FinishAction();
	}
}
