using Framework.Managers;
using UnityEngine;

namespace Framework.FrameworkCore.Attributes.Logic;

public class VariableAttribute : Attribute
{
	protected float _currentValue;

	private float _maxValue;

	private float _maxFactor = 1f;

	public float MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			_maxValue = value;
			base.Base = value;
			CalculateValue();
		}
	}

	public float MaxFactor
	{
		get
		{
			return _maxFactor;
		}
		set
		{
			_maxFactor = value;
			CalculateValue();
		}
	}

	public float Current
	{
		get
		{
			CalculateValue();
			return _currentValue;
		}
		set
		{
			float currentValue = _currentValue;
			_currentValue = value;
			CalculateValue();
			if (currentValue != _currentValue && this.OnChanged != null)
			{
				this.OnChanged();
			}
		}
	}

	public float CurrentMax => CurrentMaxWithoutFactor * MaxFactor;

	public float CurrentMaxWithoutFactor => Mathf.Min(base.CalculateValue(), MaxValue);

	public event Core.SimpleEvent OnChanged;

	public VariableAttribute(float baseValue, float upgradeValue, float maxValue, float baseMultiplier)
		: base(baseValue, upgradeValue, baseMultiplier)
	{
		_maxValue = maxValue;
		Current = baseValue;
	}

	public override bool IsVariable()
	{
		return true;
	}

	public void SetToCurrentMax()
	{
		Current = CurrentMax;
	}

	public new virtual float CalculateValue()
	{
		float currentMax = CurrentMax;
		if (_currentValue > currentMax)
		{
			_currentValue = currentMax;
		}
		return _currentValue;
	}
}
