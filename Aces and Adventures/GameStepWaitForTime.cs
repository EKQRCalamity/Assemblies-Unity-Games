using System.Collections;
using UnityEngine;

public class GameStepWaitForTime : GameStep
{
	private float _time;

	private bool _useUnscaledTime;

	public GameStepWaitForTime(float time, bool useUnscaledTime = false)
	{
		_time = time;
		_useUnscaledTime = useUnscaledTime;
	}

	protected override IEnumerator Update()
	{
		while ((_useUnscaledTime ? Time.unscaledTime : Time.time) < _time)
		{
			yield return null;
		}
	}
}
