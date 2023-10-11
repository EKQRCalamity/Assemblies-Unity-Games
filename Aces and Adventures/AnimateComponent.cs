using UnityEngine;
using UnityEngine.Events;

public abstract class AnimateComponent : MonoBehaviour
{
	[Header("Master")]
	public float loopTime = 1f;

	public float loopCount;

	public float completeDelay;

	public OnCompleteAction onCompleteAction;

	public bool useLateUpdate;

	public UnityEvent onBegin;

	public UnityEvent onComplete;

	private bool _useScaledTime = true;

	private float _elapsedTime;

	private float _elapsedRatio;

	private bool _isComplete;

	private bool _completionTriggered;

	private float _elapsedTimeSinceCompletion;

	private bool _reverse;

	private bool _initialized;

	public bool useScaledTime
	{
		get
		{
			return _useScaledTime;
		}
		set
		{
			_useScaledTime = value;
		}
	}

	public float LoopTime
	{
		get
		{
			return loopTime;
		}
		set
		{
			loopTime = value;
		}
	}

	private void Start()
	{
		_initialized = true;
		CacheInitialValues();
		_Reset();
	}

	private void OnEnable()
	{
		if (_initialized)
		{
			_Reset();
		}
	}

	public void _Reset()
	{
		_isComplete = false;
		_completionTriggered = false;
		_elapsedTime = 0f;
		_elapsedTimeSinceCompletion = 0f;
		_reverse = false;
		UniqueReset();
		UniqueUpdate(0f);
		onBegin.Invoke();
	}

	protected virtual void UniqueReset()
	{
	}

	public virtual void CacheInitialValues()
	{
	}

	private void Update()
	{
		if (!useLateUpdate)
		{
			_Update();
		}
	}

	private void _Update()
	{
		if (_completionTriggered)
		{
			return;
		}
		if (_isComplete)
		{
			_elapsedTimeSinceCompletion += (_useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime);
			return;
		}
		_elapsedTime += (_useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime);
		_elapsedRatio = _elapsedTime / loopTime;
		if (loopCount > 0f && _elapsedRatio > loopCount)
		{
			_elapsedRatio = loopCount;
			_isComplete = true;
		}
		UniqueUpdate(_reverse ? (1f - _elapsedRatio) : _elapsedRatio);
	}

	protected abstract void UniqueUpdate(float t);

	private void LateUpdate()
	{
		if (useLateUpdate)
		{
			_Update();
		}
		if (_isComplete && !_completionTriggered && _elapsedTimeSinceCompletion >= completeDelay)
		{
			_completionTriggered = true;
			if (onComplete != null)
			{
				onComplete.Invoke();
			}
			this.DoOnCompleteAction(onCompleteAction);
		}
	}

	protected float GetValue(Vector2 range, AnimationCurve curve, AnimationType animType, float t, float repeat)
	{
		int additiveCount;
		float num = GraphicsUtil.SampleAt(animType, t, repeat, out additiveCount);
		float time = curve[curve.length - 1].time;
		return Mathf.Lerp(range.x, range.y, curve.Evaluate(num * time)) + curve.Evaluate(time) * (float)additiveCount * (range.y - range.x);
	}

	protected Color GetColor(Gradient gradient, AnimationCurve curve, AnimationType animType, float t, float repeat, Color tint)
	{
		return gradient.Evaluate(GetValue(Vector2.up, curve, animType, t, repeat)) * tint;
	}

	public void Reverse()
	{
		_Reset();
		_reverse = true;
		_Update();
	}

	public void DestroySelf()
	{
		Object.Destroy(base.gameObject);
	}
}
