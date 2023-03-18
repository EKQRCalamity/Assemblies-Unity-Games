using UnityEngine;

namespace Framework.Util;

public class Timer
{
	private float _timeElapsed;

	private float _totalTime;

	public float TotalTime
	{
		get
		{
			return _totalTime;
		}
		set
		{
			_totalTime = value;
		}
	}

	public float Elapsed => Mathf.Clamp(_timeElapsed / _totalTime, 0f, 1f);

	public Timer(float timeToCountInSec)
	{
		_totalTime = timeToCountInSec;
	}

	public bool UpdateAndTest()
	{
		_timeElapsed += Time.unscaledDeltaTime;
		return _timeElapsed >= _totalTime;
	}
}
