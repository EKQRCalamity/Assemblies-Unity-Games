using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.IA;

public class WaitSeconds_EnemyAction : EnemyAction
{
	private float minSeconds;

	private float maxSeconds;

	public EnemyAction StartAction(EnemyBehaviour e, float _minSeconds, float _maxSeconds)
	{
		minSeconds = _minSeconds;
		maxSeconds = _maxSeconds;
		return StartAction(e);
	}

	public EnemyAction StartAction(EnemyBehaviour e, float _seconds)
	{
		return StartAction(e, _seconds, _seconds);
	}

	protected override IEnumerator BaseCoroutine()
	{
		float waitSeconds = Random.Range(minSeconds, maxSeconds);
		yield return new WaitForSeconds(waitSeconds);
		FinishAction();
	}
}
