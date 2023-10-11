using UnityEngine;

public class ToggleFloat : MonoBehaviour
{
	public float offValue;

	public float onValue = 1f;

	public AnimationCurve offToOnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[Range(0.001f, 2f)]
	public float toggleTime = 0.2f;

	public bool autoCompleteOnEnable;

	public bool useScaledTime;

	private bool? _toggle;

	private float _t;

	private bool _finished = true;

	private bool _enabledThisFrame = true;

	public FloatEvent OnValueChanged;

	public BoolEvent OnToggleChanged;

	public bool toggle
	{
		get
		{
			if (_toggle.HasValue)
			{
				return _toggle.Value;
			}
			return false;
		}
		set
		{
			OnToggle(value);
		}
	}

	private void _Finish(bool isOn)
	{
		_t = isOn.ToInt();
		_finished = true;
		UnityEventExtensions.SafeInvoke(ref OnToggleChanged, isOn);
	}

	private void _SignalValueChange()
	{
		UnityEventExtensions.SafeInvoke(ref OnValueChanged, Mathf.Lerp(offValue, onValue, offToOnCurve.Evaluate(Mathf.Clamp01(_t))));
	}

	private void OnDisable()
	{
		_toggle = null;
		_t = 0f;
		_finished = true;
		_enabledThisFrame = true;
	}

	private void Update()
	{
		if (!_finished && _toggle.HasValue)
		{
			bool value = _toggle.Value;
			bool flag = !_enabledThisFrame || autoCompleteOnEnable;
			if (_enabledThisFrame && autoCompleteOnEnable)
			{
				_t = value.ToInt();
			}
			float num = GameUtil.GetDeltaTime(useScaledTime) / toggleTime;
			if (value && (_t += num) >= 1f && flag)
			{
				_Finish(isOn: true);
			}
			else if (!value && (_t -= num) <= 0f && flag)
			{
				_Finish(isOn: false);
			}
			_SignalValueChange();
		}
	}

	private void LateUpdate()
	{
		_enabledThisFrame = false;
	}

	public void OnToggle(bool toggle)
	{
		if (_toggle != toggle)
		{
			_toggle = toggle;
			_finished = false;
			Update();
		}
	}

	public void OnToggleActivate(bool toggle)
	{
		if (_toggle != toggle)
		{
			if (toggle && !base.isActiveAndEnabled)
			{
				OnToggleChanged.Invoke(arg0: true);
			}
			OnToggle(toggle);
		}
	}

	public void ForceFinish()
	{
		if (_toggle.HasValue)
		{
			_Finish(_toggle.Value);
			_SignalValueChange();
		}
	}
}
