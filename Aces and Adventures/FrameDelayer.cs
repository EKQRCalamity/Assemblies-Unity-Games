using UnityEngine;
using UnityEngine.Events;

public class FrameDelayer : MonoBehaviour
{
	[Range(-1f, 1000f)]
	public int framesToDelayOnEnable = 1;

	[SerializeField]
	protected UnityEvent _OnDelayComplete;

	private int? _framesRemaining;

	public UnityEvent OnDelayComplete => _OnDelayComplete ?? (_OnDelayComplete = new UnityEvent());

	private void OnEnable()
	{
		StartTimer(framesToDelayOnEnable);
	}

	private void Update()
	{
		if (_framesRemaining.HasValue)
		{
			_framesRemaining--;
			if (!(_framesRemaining >= 0))
			{
				OnDelayComplete.Invoke();
				_framesRemaining = null;
			}
		}
	}

	public void StartTimer(int frames)
	{
		if (frames >= 0)
		{
			_framesRemaining = frames;
		}
	}
}
