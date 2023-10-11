using System;
using System.Collections;
using UnityEngine;

public class GameStepDelayedAction : GameStep
{
	private float _delay;

	private Action _delayedAction;

	public GameStepDelayedAction(float delay, Action delayedAction)
	{
		_delay = delay;
		_delayedAction = delayedAction;
	}

	protected override IEnumerator Update()
	{
		while ((_delay -= Time.deltaTime) > 0f)
		{
			yield return null;
		}
	}

	protected override void End()
	{
		_delayedAction?.Invoke();
	}
}
