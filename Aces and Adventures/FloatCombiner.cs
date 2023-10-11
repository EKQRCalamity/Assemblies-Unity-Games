using System;
using UnityEngine;

public class FloatCombiner : MonoBehaviour
{
	public enum FunctionType
	{
		Add,
		Multiply,
		Divide
	}

	public FunctionType function;

	[SerializeField]
	protected float[] _floats;

	public FloatEvent onResultChange;

	private float _result;

	public float value0
	{
		get
		{
			return _floats[0];
		}
		set
		{
			SetValue(0, value);
		}
	}

	public float value1
	{
		get
		{
			return _floats[1];
		}
		set
		{
			SetValue(1, value);
		}
	}

	public float value2
	{
		get
		{
			return _floats[2];
		}
		set
		{
			SetValue(2, value);
		}
	}

	public float value3
	{
		get
		{
			return _floats[3];
		}
		set
		{
			SetValue(3, value);
		}
	}

	public float value4
	{
		get
		{
			return _floats[4];
		}
		set
		{
			SetValue(4, value);
		}
	}

	public float value5
	{
		get
		{
			return _floats[5];
		}
		set
		{
			SetValue(5, value);
		}
	}

	public float result
	{
		get
		{
			return _result;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _result, value))
			{
				onResultChange?.Invoke(value);
			}
		}
	}

	public void SetValue(int index, float value)
	{
		if (!SetPropertyUtility.SetStruct(ref _floats[index], value))
		{
			return;
		}
		float num = 1f;
		switch (function)
		{
		case FunctionType.Add:
		{
			num = 0f;
			for (int j = 0; j < _floats.Length; j++)
			{
				num += _floats[j];
			}
			break;
		}
		case FunctionType.Multiply:
		{
			for (int k = 0; k < _floats.Length; k++)
			{
				num *= _floats[k];
			}
			break;
		}
		case FunctionType.Divide:
		{
			for (int i = 0; i < _floats.Length; i++)
			{
				num /= _floats[i].InsureNonZero();
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
		result = num;
	}
}
