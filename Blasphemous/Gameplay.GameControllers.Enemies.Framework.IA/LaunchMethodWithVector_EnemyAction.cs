using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class LaunchMethodWithVector_EnemyAction : EnemyAction
{
	private Action<Vector2> method;

	private Vector2 vector = Vector2.zero;

	public EnemyAction StartAction(EnemyBehaviour e, Action<Vector2> method, Vector2 vector)
	{
		this.method = method;
		this.vector = vector;
		return StartAction(e);
	}

	protected override void DoOnStart()
	{
		base.DoOnStart();
		method(vector);
	}

	protected override IEnumerator BaseCoroutine()
	{
		yield return null;
		FinishAction();
	}
}
