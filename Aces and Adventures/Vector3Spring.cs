using UnityEngine;

public class Vector3Spring : Vector2Spring
{
	[Header("Z")]
	[Range(0f, 1000f)]
	public float springConstantZ = 100f;

	[Range(0f, 100f)]
	public float springDampeningZ = 10f;

	public FloatEvent OnZChange;

	public Vector3Event OnVector3Change;

	[SerializeField]
	[HideInInspector]
	protected float _z;

	[SerializeField]
	[HideInInspector]
	protected float _zDesired;

	[SerializeField]
	[HideInInspector]
	protected float _zVelocity;

	[SerializeField]
	[HideInInspector]
	protected bool _zDirty;

	public float z
	{
		get
		{
			return _z;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _z, value))
			{
				_zDirty = true;
			}
		}
	}

	public float zDesired
	{
		get
		{
			return _zDesired;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _zDesired, value))
			{
				_zDirty = true;
			}
		}
	}

	protected override bool finished
	{
		get
		{
			if (base.finished)
			{
				return !_zDirty;
			}
			return false;
		}
	}

	protected override bool _IsZeroReached()
	{
		if (!base._IsZeroReached())
		{
			if (_zDesired == 0f)
			{
				return _z <= 0f;
			}
			return false;
		}
		return true;
	}

	protected override void _OnZeroReached()
	{
		_z = 0f;
		OnZChange.Invoke(0f);
		base._OnZeroReached();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		_zVelocity = 0f;
		_zDirty = true;
	}

	protected override void Update()
	{
		bool xDirty = _xDirty;
		bool yDirty = _yDirty;
		bool zDirty = _zDirty;
		base.Update();
		if (_zDirty)
		{
			_zDirty = _Spring(ref _z, ref _zVelocity, _zDesired, springConstantZ, springDampeningZ, OnZChange);
		}
		if (xDirty || yDirty || zDirty)
		{
			OnVector3Change.Invoke(new Vector3(_x, _y, _z));
		}
	}

	public void SetValue(Vector3 value)
	{
		base.SetValue(value.Project(AxisType.Z));
		_zVelocity = 0f;
		z = value.z;
		zDesired = value.z;
		OnZChange.Invoke(value.z);
		OnVector3Change.Invoke(value);
	}
}
