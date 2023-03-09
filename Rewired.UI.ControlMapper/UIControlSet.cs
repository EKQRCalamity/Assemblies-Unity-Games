using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper;

[AddComponentMenu("")]
public class UIControlSet : MonoBehaviour
{
	[SerializeField]
	private Text title;

	private Dictionary<int, UIControl> _controls;

	private Dictionary<int, UIControl> controls => _controls ?? (_controls = new Dictionary<int, UIControl>());

	public void SetTitle(string text)
	{
		if (!(title == null))
		{
			title.text = text;
		}
	}

	public T GetControl<T>(int uniqueId) where T : UIControl
	{
		controls.TryGetValue(uniqueId, out var value);
		return value as T;
	}

	public UISliderControl CreateSlider(GameObject prefab, Sprite icon, float minValue, float maxValue, Action<int, float> valueChangedCallback, Action<int> cancelCallback)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		UISliderControl control = gameObject.GetComponent<UISliderControl>();
		if (control == null)
		{
			UnityEngine.Object.Destroy(gameObject);
			UnityEngine.Debug.LogError("Prefab missing UISliderControl component!");
			return null;
		}
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		if (control.iconImage != null)
		{
			control.iconImage.sprite = icon;
		}
		if (control.slider != null)
		{
			control.slider.minValue = minValue;
			control.slider.maxValue = maxValue;
			if (valueChangedCallback != null)
			{
				control.slider.onValueChanged.AddListener(delegate(float value)
				{
					valueChangedCallback(control.id, value);
				});
			}
			if (cancelCallback != null)
			{
				control.SetCancelCallback(delegate
				{
					cancelCallback(control.id);
				});
			}
		}
		controls.Add(control.id, control);
		return control;
	}
}
