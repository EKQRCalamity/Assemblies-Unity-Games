using UnityEngine;

public class Vector2Spring : FloatSpring
{
	[Header("Y")]
	[Range(0f, 1000f)]
	public float springConstantY = 100f;

	[Range(0f, 100f)]
	public float springDampeningY = 10f;

	public FloatEvent OnYChange;

	public Vector2Event OnVector2Change;

	[SerializeField]
	[HideInInspector]
	protected float _y;

	[SerializeField]
	[HideInInspector]
	protected float _yDesired;

	[SerializeField]
	[HideInInspector]
	protected float _yVelocity;

	[SerializeField]
	[HideInInspector]
	protected bool _yDirty;

	public float y
	{
		get
		{
			return _y;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _y, value))
			{
				_yDirty = true;
			}
		}
	}

	public float yDesired
	{
		get
		{
			return _yDesired;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _yDesired, value))
			{
				_yDirty = true;
			}
		}
	}

	protected override bool finished
	{
		get
		{
			if (base.finished)
			{
				return !_yDirty;
			}
			return false;
		}
	}

	protected override bool _IsZeroReached()
	{
		if (!base._IsZeroReached())
		{
			if (_yDesired == 0f)
			{
				return _y <= 0f;
			}
			return false;
		}
		return true;
	}

	protected override void _OnZeroReached()
	{
		_y = 0f;
		OnYChange.Invoke(0f);
		base._OnZeroReached();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_yVelocity = 0f;
		_yDirty = true;
	}

	protected override void Update()
	{
		bool xDirty = _xDirty;
		bool yDirty = _yDirty;
		base.Update();
		if (_yDirty)
		{
			_yDirty = _Spring(ref _y, ref _yVelocity, _yDesired, springConstantY, springDampeningY, OnYChange);
		}
		if (xDirty || yDirty)
		{
			OnVector2Change.Invoke(new Vector2(_x, _y));
		}
	}

	public void SetValue(Vector2 value)
	{
		SetValue(value.x);
		_yVelocity = 0f;
		y = value.y;
		yDesired = value.y;
		OnYChange.Invoke(value.y);
		OnVector2Change.Invoke(value);
	}
}
