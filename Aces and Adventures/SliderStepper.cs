using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
[DisallowMultipleComponent]
public class SliderStepper : MonoBehaviour
{
	public float stepSize;

	private Slider slider;

	private int frameOfLastStep;

	private void Start()
	{
		frameOfLastStep = -1;
		slider = GetComponent<Slider>();
		Step(slider.value);
		slider.onValueChanged.AddListener(Step);
	}

	public void Step(float value)
	{
		float num = MathUtil.RoundToNearestMultipleOf(value, stepSize);
		if (Math.Abs(num - slider.value) > float.Epsilon && frameOfLastStep != Time.frameCount)
		{
			frameOfLastStep = Time.frameCount;
			slider.value = num;
		}
	}
}
