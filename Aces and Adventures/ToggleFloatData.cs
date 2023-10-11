using UnityEngine;

public struct ToggleFloatData
{
	public const float DEFAULT_TIME = 0.15f;

	private static readonly AnimationCurve OffToOnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public static ToggleFloatData Open = new ToggleFloatData(0.15f, 1f, toggle: true, finished: true);

	public static ToggleFloatData Close = new ToggleFloatData(0.15f, 0f, toggle: false, finished: true);

	private float _toggleTime;

	private float _t;

	private bool _toggle;

	private bool _finished;

	public float value => OffToOnCurve.Evaluate(_t);

	public bool isOn
	{
		get
		{
			return _toggle;
		}
		set
		{
			_toggle = value;
			_finished = false;
		}
	}

	public ToggleFloatData(float toggleTime)
	{
		_toggleTime = toggleTime;
		_t = 0f;
		_toggle = false;
		_finished = true;
	}

	private ToggleFloatData(float toggleTime, float t, bool toggle, bool finished)
	{
		_toggleTime = toggleTime;
		_t = t;
		_toggle = toggle;
		_finished = finished;
	}

	public bool Update()
	{
		if (_finished)
		{
			return false;
		}
		float num = Time.unscaledDeltaTime / _toggleTime;
		if (_toggle && _t <= 1f)
		{
			_t += num;
			if (_t > 1f)
			{
				_t = 1f;
				_finished = true;
			}
		}
		else if (!_toggle && _t >= 0f)
		{
			_t -= num;
			if (_t < 0f)
			{
				_t = 0f;
				_finished = true;
			}
		}
		return true;
	}

	public static implicit operator bool(ToggleFloatData toggleFloat)
	{
		return toggleFloat._toggle;
	}

	public override string ToString()
	{
		return $"ToggleTime: {_toggleTime}, T: {_t}, Toggle: {_toggle}, Finished: {_finished}";
	}
}
