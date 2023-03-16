using System.Collections;
using UnityEngine;

public class WaitForFrameTimePersistent : IEnumerator
{
	private bool useUnalteredTime;

	public float accumulator { get; protected set; }

	public float frameTime { get; protected set; }

	public float totalDelta => frameTime + accumulator;

	public virtual object Current => null;

	public WaitForFrameTimePersistent(float frameTime, bool useUnalteredTime = false)
	{
		this.frameTime = frameTime;
		this.useUnalteredTime = useUnalteredTime;
	}

	public bool MoveNext()
	{
		accumulator += deltaTime();
		bool flag = accumulator >= frameTime;
		if (flag)
		{
			accumulator -= Mathf.Floor(accumulator / frameTime) * frameTime;
		}
		return !flag;
	}

	protected virtual float deltaTime()
	{
		return (!useUnalteredTime) ? ((float)CupheadTime.Delta) : Time.deltaTime;
	}

	public void Reset()
	{
		accumulator = 0f;
	}
}
