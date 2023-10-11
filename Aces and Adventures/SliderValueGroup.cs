using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueGroup : MonoBehaviour
{
	public float minTotalValue;

	public float maxTotalValue;

	public List<Slider> sliders;

	private float previousTotal;

	public FloatEvent onTotalChanged;

	private void Awake()
	{
		for (int i = 0; i < sliders.Count; i++)
		{
			Slider slider = sliders[i];
			slider.onValueChanged.AddListener(delegate
			{
				float currentTotal = GetCurrentTotal();
				if (currentTotal < minTotalValue)
				{
					float num = minTotalValue - currentTotal;
					slider.value += num;
				}
				else if (currentTotal > maxTotalValue)
				{
					float num2 = maxTotalValue - currentTotal;
					slider.value += num2;
				}
			});
			slider.onValueChanged.Invoke(slider.value);
		}
	}

	public float GetCurrentTotal()
	{
		float num = 0f;
		for (int i = 0; i < sliders.Count; i++)
		{
			num += sliders[i].value;
		}
		if (num != previousTotal && onTotalChanged != null)
		{
			onTotalChanged.Invoke(num);
		}
		previousTotal = num;
		return num;
	}
}
