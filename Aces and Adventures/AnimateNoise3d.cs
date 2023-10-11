using System;
using UnityEngine;

public class AnimateNoise3d : MonoBehaviour
{
	private static System.Random _random;

	public bool useScaledTime = true;

	public float rangeMultiplier = 1f;

	public float frequencyMultiplier = 1f;

	[Header("X")]
	public Vector2 xRange = new Vector2(-1f, 1f);

	public float xFrequency = 1f;

	[Header("Y")]
	public Vector2 yRange = new Vector2(-1f, 1f);

	public float yFrequency = 1f;

	[Header("Z")]
	public Vector2 zRange = new Vector2(-1f, 1f);

	public float zFrequency = 1f;

	[Header("Events")]
	public Vector3Event OnValueChange;

	private float _elapsedTime;

	private Vector2 _noiseOffset;

	private static System.Random random => _random ?? (_random = new System.Random());

	public Vector3 value => new Vector3(_GetValue(xRange, xFrequency, _elapsedTime, Vector2.right), _GetValue(yRange, yFrequency, _elapsedTime, Vector2.up), _GetValue(zRange, zFrequency, _elapsedTime, Vector2.left));

	private float _GetValue(Vector2 range, float frequency, float time, Vector2 noiseDirection)
	{
		Vector2 vector = noiseDirection * frequency * time;
		return range.Lerp(Mathf.PerlinNoise(_noiseOffset.x + vector.x, _noiseOffset.y + vector.y)) * rangeMultiplier;
	}

	private void OnEnable()
	{
		_elapsedTime = 0f;
		_noiseOffset = new Vector2(random.Next(2048), random.Next(2048));
	}

	private void Update()
	{
		_elapsedTime += GameUtil.GetDeltaTime(useScaledTime) * frequencyMultiplier;
		OnValueChange.Invoke(value);
	}

	public void SetRangeMultiplier(float multiplier)
	{
		rangeMultiplier = multiplier;
	}

	public void SetFrequencyMultiplier(float multiplier)
	{
		frequencyMultiplier = multiplier;
	}
}
