using UnityEngine;
using UnityEngine.UI;

public class SliderMinMaxLink : MonoBehaviour
{
	public float min;

	public float max;

	public Slider minSlider;

	public Slider maxSlider;

	public bool setRangeOnAwake = true;

	public Vector2Event onRangeChanged;

	private void Awake()
	{
		minSlider.minValue = min;
		minSlider.maxValue = max;
		maxSlider.minValue = min;
		maxSlider.maxValue = max;
		minSlider.onValueChanged.AddListener(OnMinValueChanged);
		maxSlider.onValueChanged.AddListener(OnMaxValueChanged);
		if (setRangeOnAwake)
		{
			minSlider.value = min;
			maxSlider.value = max;
			onRangeChanged.Invoke(new Vector2(min, max));
		}
	}

	private void OnValidate()
	{
		if ((bool)minSlider)
		{
			minSlider.minValue = min;
			minSlider.maxValue = max;
			minSlider.value = min;
		}
		if ((bool)maxSlider)
		{
			maxSlider.minValue = min;
			maxSlider.maxValue = max;
			maxSlider.value = max;
		}
	}

	private void OnMinValueChanged(float value)
	{
		if (value > maxSlider.value)
		{
			maxSlider.value = value;
		}
		if (onRangeChanged != null)
		{
			onRangeChanged.Invoke(new Vector2(minSlider.value, maxSlider.value));
		}
	}

	private void OnMaxValueChanged(float value)
	{
		if (value < minSlider.value)
		{
			minSlider.value = value;
		}
		if (onRangeChanged != null)
		{
			onRangeChanged.Invoke(new Vector2(minSlider.value, maxSlider.value));
		}
	}
}
