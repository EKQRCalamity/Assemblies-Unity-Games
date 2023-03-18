using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class LaunchMethodWithTwoVectors_EnemyAction : EnemyAction
{
	private Action<Vector2, Vector2> method;

	private Vector2 firstVector = Vector2.zero;

	private Vector2 lastVector = Vector2.zero;

	public EnemyAction StartAction(EnemyBehaviour e, Action<Vector2, Vector2> method, Vector2 firstVector, Vector2 lastVector)
	{
		this.method = method;
		this.firstVector = firstVector;
		this.lastVector = lastVector;
		return StartAction(e);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
		method(firstVector, lastVector);
	}

	protected override IEnumerator BaseCoroutine()
	{
		yield return null;
		FinishAction();
	}
}
