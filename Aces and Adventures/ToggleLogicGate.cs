using System;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLogicGate : MonoBehaviour
{
	[SerializeField]
	protected LogicGateType _gateType;

	[SerializeField]
	[Range(1f, 5f)]
	protected int _gateSize = 2;

	[SerializeField]
	protected BoolEvent _OnResultChanged;

	private bool?[] _values;

	private bool? _previousResult;

	protected bool?[] values => CollectionUtil.Resize(ref _values, gateSize);

	public LogicGateType gateType
	{
		get
		{
			return _gateType;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gateType, value))
			{
				_CalculateResult();
			}
		}
	}

	public int gateSize
	{
		get
		{
			return _gateSize;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _gateSize, Math.Max(1, value)))
			{
				_CalculateResult();
			}
		}
	}

	public BoolEvent OnResultChanged => _OnResultChanged ?? (_OnResultChanged = new BoolEvent());

	protected bool? previousResult
	{
		get
		{
			return _previousResult;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _previousResult, value))
			{
				OnResultChanged.Invoke(value.GetValueOrDefault());
			}
		}
	}

	public bool value0
	{
		get
		{
			return this[0];
		}
		set
		{
			this[0] = value;
		}
	}

	public bool value1
	{
		get
		{
			return this[1];
		}
		set
		{
			this[1] = value;
		}
	}

	public bool value2
	{
		get
		{
			return this[2];
		}
		set
		{
			this[2] = value;
		}
	}

	public bool value3
	{
		get
		{
			return this[3];
		}
		set
		{
			this[3] = value;
		}
	}

	public bool value4
	{
		get
		{
			return this[4];
		}
		set
		{
			this[4] = value;
		}
	}

	public bool this[int index]
	{
		get
		{
			return values[index].GetValueOrDefault();
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref values[index], value))
			{
				_CalculateResult();
			}
		}
	}

	private void _CalculateResult()
	{
		int num = 0;
		for (int i = 0; i < _gateSize; i++)
		{
			num += this[i].ToInt();
		}
		previousResult = gateType switch
		{
			LogicGateType.Any => num > 0, 
			LogicGateType.All => num == _gateSize, 
			LogicGateType.OnlyOne => num == 1, 
			LogicGateType.None => num == 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void SetValues(List<bool> newValues)
	{
		_gateSize = newValues.Count;
		for (int i = 0; i < _gateSize; i++)
		{
			values[i] = newValues[i];
		}
		_CalculateResult();
	}
}
