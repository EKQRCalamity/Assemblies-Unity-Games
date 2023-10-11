using System;
using UnityEngine;

public class BarFiller : MonoBehaviour
{
	[SerializeField]
	protected float _min;

	[SerializeField]
	protected float _max = 1f;

	[SerializeField]
	protected float _value;

	[Header("Events")]
	public FloatEvent onValueChange;

	public FloatEvent onNormalizedValueChange;

	public BoolEvent onIsFillingChange;

	public BoolEvent onIsEmptyingChange;

	private int _previousDirection;

	private int _valueChangeDirection;

	private int _fillDirection;

	public float min
	{
		get
		{
			return _min;
		}
		set
		{
			float num = normalizedValue;
			if (SetPropertyUtility.SetStruct(ref _min, value))
			{
				if (_value < min)
				{
					_value = min;
					onValueChange?.Invoke(_value);
				}
				float num2 = normalizedValue;
				if (num != num2)
				{
					onNormalizedValueChange?.Invoke(num2);
				}
			}
		}
	}

	public float max
	{
		get
		{
			return _max;
		}
		set
		{
			float num = normalizedValue;
			if (SetPropertyUtility.SetStruct(ref _max, value))
			{
				if (_value > max)
				{
					_value = max;
					onValueChange?.Invoke(_value);
				}
				float num2 = normalizedValue;
				if (num != num2)
				{
					onNormalizedValueChange?.Invoke(num2);
				}
			}
		}
	}

	public float value
	{
		get
		{
			return _value;
		}
		set
		{
			value = Mathf.Clamp(value, min, max);
			float num = _value;
			if (SetPropertyUtility.SetStruct(ref _value, value))
			{
				_valueChangeDirection = Math.Sign(value - num);
				onValueChange?.Invoke(_value);
				onNormalizedValueChange?.Invoke(normalizedValue);
			}
		}
	}

	public float normalizedValue
	{
		get
		{
			return MathUtil.GetLerpAmount(min, max, value);
		}
		set
		{
			this.value = Mathf.Lerp(min, max, value);
		}
	}

	private int fillDirection
	{
		get
		{
			return _fillDirection;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _fillDirection, value))
			{
				onIsFillingChange?.Invoke(value > 0);
				onIsEmptyingChange?.Invoke(value < 0);
			}
		}
	}

	private void LateUpdate()
	{
		if (_valueChangeDirection == _previousDirection)
		{
			fillDirection = _valueChangeDirection;
		}
		_previousDirection = _valueChangeDirection;
		_valueChangeDirection = 0;
	}
}
