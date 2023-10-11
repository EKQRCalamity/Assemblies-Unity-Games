using UnityEngine;
using UnityEngine.Events;

public class Delayer : MonoBehaviour
{
	public bool useScaledTime;

	public float delayTime;

	public bool startTimerOnEnable;

	private float _delayRemaining;

	public UnityEvent OnDelayComplete;

	private void OnEnable()
	{
		if (startTimerOnEnable)
		{
			StartTimer();
		}
	}

	private void Update()
	{
		if (!(_delayRemaining <= 0f))
		{
			_delayRemaining -= GameUtil.GetDeltaTime(useScaledTime);
			if (_delayRemaining <= 0f)
			{
				OnDelayComplete.Invoke();
			}
		}
	}

	public void StartTimer()
	{
		_delayRemaining = Mathf.Max(float.Epsilon, delayTime);
	}

	public void StartTimer(float delayTime)
	{
		this.delayTime = delayTime;
		StartTimer();
	}
}
