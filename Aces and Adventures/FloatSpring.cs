using UnityEngine;
using UnityEngine.Events;

public class FloatSpring : MonoBehaviour
{
	protected const float MIN_CONSTANT = 0f;

	protected const float MAX_CONSTANT = 1000f;

	protected const float DEF_CONSTANT = 100f;

	protected const float MIN_DAMP = 0f;

	protected const float MAX_DAMP = 100f;

	protected const float DEF_DAMP = 10f;

	protected const float SNAP_DISTANCE_THRESHOLD = 0.001f;

	protected const float SNAP_VELOCITY_THRESHOLD = 0.1f;

	[Header("Common")]
	public bool useScaledTime = true;

	[SerializeField]
	protected bool _signalZeroReached;

	public UnityEvent OnZeroReached;

	public UnityEvent OnFinished;

	[Header("X")]
	[Range(0f, 1000f)]
	public float springConstantX = 100f;

	[Range(0f, 100f)]
	public float springDampeningX = 10f;

	public FloatEvent OnXChange;

	[Range(0.01f, 100f)]
	public float thresholdMultiplier = 1f;

	[SerializeField]
	[HideInInspector]
	protected float _x;

	[SerializeField]
	[HideInInspector]
	protected float _xDesired;

	[SerializeField]
	[HideInInspector]
	protected float _xVelocity;

	[SerializeField]
	[HideInInspector]
	protected bool _xDirty;

	private bool _wasFinished;

	public bool signalZeroReached
	{
		get
		{
			return _signalZeroReached;
		}
		set
		{
			_signalZeroReached = value;
		}
	}

	public float x
	{
		get
		{
			return _x;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _x, value))
			{
				_xDirty = true;
			}
		}
	}

	public float xDesired
	{
		get
		{
			return _xDesired;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _xDesired, value))
			{
				_xDirty = true;
			}
		}
	}

	public float xVelocity
	{
		get
		{
			return _xVelocity;
		}
		set
		{
			_xDirty |= SetPropertyUtility.SetStruct(ref _xVelocity, value);
		}
	}

	protected virtual bool finished => !_xDirty;

	protected bool _Spring(ref float value, ref float velocity, float target, float springConstant, float springDampening, FloatEvent onChange)
	{
		MathUtil.Spring(ref value, ref velocity, target, springConstant, springDampening, GameUtil.GetDeltaTime(useScaledTime));
		if (Mathf.Abs(velocity) <= 0.1f * thresholdMultiplier && Mathf.Abs(target - value) <= 0.001f * thresholdMultiplier)
		{
			value = target;
			velocity = 0f;
		}
		onChange.Invoke(value);
		if (value == target)
		{
			return velocity != 0f;
		}
		return true;
	}

	protected virtual bool _IsZeroReached()
	{
		if (_xDesired == 0f)
		{
			return _x <= 0f;
		}
		return false;
	}

	protected virtual void _OnZeroReached()
	{
		_x = 0f;
		OnXChange.Invoke(0f);
		OnZeroReached.Invoke();
	}

	protected virtual void OnEnable()
	{
		_xVelocity = 0f;
		_xDirty = true;
	}

	protected virtual void Update()
	{
		_wasFinished = finished;
		if (_xDirty)
		{
			_xDirty = _Spring(ref _x, ref _xVelocity, _xDesired, springConstantX, springDampeningX, OnXChange);
			if (signalZeroReached && _IsZeroReached())
			{
				_OnZeroReached();
			}
		}
	}

	protected void LateUpdate()
	{
		if (!_wasFinished && finished)
		{
			OnFinished.Invoke();
		}
	}

	public void SetValue(float value)
	{
		_xVelocity = 0f;
		x = value;
		xDesired = value;
		OnXChange.Invoke(value);
	}

	public void AddVelocity(float velocity)
	{
		xVelocity += velocity;
	}
}
