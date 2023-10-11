using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleAnimHook : MonoBehaviour
{
	public string animParamName;

	private Toggle _toggle;

	public StringBoolEvent onToggle;

	public UnityEvent onToggleOn;

	public UnityEvent onToggleOff;

	public BoolEvent onValueChangedInverse;

	private Toggle toggle
	{
		get
		{
			if (!_toggle)
			{
				Awake();
			}
			return _toggle;
		}
	}

	private void Awake()
	{
		if ((bool)_toggle)
		{
			return;
		}
		_toggle = GetComponent<Toggle>();
		_toggle.onValueChanged.AddListener(delegate(bool b)
		{
			if (onToggle != null)
			{
				onToggle.Invoke(animParamName, b);
			}
			if (b && onToggleOn != null)
			{
				onToggleOn.Invoke();
			}
			if (!b && onToggleOff != null)
			{
				onToggleOff.Invoke();
			}
			if (onValueChangedInverse != null)
			{
				onValueChangedInverse.Invoke(!b);
			}
		});
	}

	public void Toggle()
	{
		toggle.isOn = !toggle.isOn;
	}

	public void ToggleOffOnly(bool isOn)
	{
		if (!isOn)
		{
			toggle.isOn = false;
		}
	}

	public void RefreshToggleValue()
	{
		toggle.isOn = !toggle.isOn;
		toggle.isOn = !toggle.isOn;
	}

	public void InvokeToggleOn()
	{
		onToggleOn.Invoke();
	}
}
