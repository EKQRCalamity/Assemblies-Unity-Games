using System;
using UnityEngine;

public class FloatWave : MonoBehaviour
{
	public Vector2 range = new Vector2(0f, 1f);

	public bool useScaledTime;

	[Range(0.001f, 10f)]
	public float frequency;

	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private float _elapsedTime;

	public FloatEvent OnValueChange;

	public FloatEvent OnValueChangeInverse;

	public float rangeMin
	{
		get
		{
			return range.x;
		}
		set
		{
			range.x = value;
		}
	}

	public float rangeMax
	{
		get
		{
			return range.y;
		}
		set
		{
			range.y = value;
		}
	}

	private void OnEnable()
	{
		_elapsedTime = 0f;
		Update();
	}

	private void Update()
	{
		float time = (Mathf.Cos(_elapsedTime * frequency * MathF.PI - MathF.PI) + 1f) * 0.5f;
		time = curve.Evaluate(time);
		OnValueChange.Invoke(Mathf.Lerp(range.x, range.y, time));
		OnValueChangeInverse.Invoke(Mathf.Lerp(range.x, range.y, 1f - time));
		_elapsedTime += GameUtil.GetDeltaTime(useScaledTime);
	}
}
