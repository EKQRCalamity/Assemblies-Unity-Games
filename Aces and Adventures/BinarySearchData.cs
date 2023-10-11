using System;
using ProtoBuf;

[ProtoContract]
public class BinarySearchData
{
	[ProtoMember(1)]
	private float _currentValue;

	[ProtoMember(2)]
	private float? _minima;

	[ProtoMember(3)]
	private float? _maxima;

	public float currentValue => _currentValue;

	public float windowedValue
	{
		get
		{
			return _currentValue;
		}
		set
		{
			if (_minima.HasValue)
			{
				value = Math.Max(_minima.Value, value);
				_minima = Math.Min(_minima.Value, value);
			}
			if (_maxima.HasValue)
			{
				value = Math.Min(_maxima.Value, value);
				_maxima = Math.Max(_maxima.Value, value);
			}
			_currentValue = value;
		}
	}

	private BinarySearchData()
	{
	}

	public BinarySearchData(float initialValue, float? minima = null, float? maxima = null)
	{
		Reset(initialValue, minima, maxima);
	}

	public void Up()
	{
		_minima = _currentValue;
		if (_maxima.HasValue)
		{
			_currentValue = (_maxima.Value + _currentValue) * 0.5f;
		}
		else
		{
			_currentValue += _currentValue;
		}
	}

	public void Down()
	{
		_maxima = _currentValue;
		if (_minima.HasValue)
		{
			_currentValue = (_minima.Value + _currentValue) * 0.5f;
		}
		else
		{
			_currentValue *= 0.5f;
		}
	}

	public void Reset(float initialValue, float? minima = null, float? maxima = null)
	{
		_currentValue = initialValue;
		_minima = minima;
		_maxima = maxima;
	}

	public static implicit operator float(BinarySearchData data)
	{
		return data._currentValue;
	}
}
