using System;
using UnityEngine;

public class FloatBouncer : MonoBehaviour
{
	private const float MAX_STEP_TIME = 1f / 60f;

	public float startingHeight = 1f;

	public float peakHeight = 2f;

	public float groundHeight = 1f;

	[Range(-100f, 100f)]
	public float gravity = -9.81f;

	[Range(0f, 100f)]
	[Space]
	public int numBounces = 3;

	[Range(0.001f, 0.5f)]
	public float finalBounceAmount = 0.1f;

	[Space]
	public bool resetOnEnable;

	public bool bounceOnEnable;

	public bool useScaledTime;

	[Space]
	public FloatEvent OnValueChanged;

	public FloatEvent OnBounce;

	private bool _bouncing;

	private float _position;

	private float _velocity;

	private float _bounciness;

	private int _bounceCount;

	private float _initialVelocity => (0f - gravity) * Mathf.Sqrt(2f * (startingHeight - peakHeight) / gravity);

	private float _initialBounciness
	{
		get
		{
			if (numBounces != 0)
			{
				return Mathf.Pow(finalBounceAmount, 1f / (float)numBounces);
			}
			return 0f;
		}
	}

	private void _UpdateStep(float deltaTime)
	{
		_velocity += gravity * deltaTime;
		int num = Math.Sign(_velocity);
		float num2 = groundHeight - _position;
		float num3 = ((num == Math.Sign(num2)) ? Mathf.Min(deltaTime, num2 / _velocity.InsureNonZero()) : deltaTime);
		_position += _velocity * num3;
		if (_position <= groundHeight && num == Math.Sign(gravity))
		{
			_bounceCount++;
			_velocity *= 0f - _bounciness;
			_position += _velocity * (deltaTime - num3);
			OnBounce.Invoke(Mathf.Pow(_bounciness, _bounceCount));
		}
		_bouncing = _bounceCount <= numBounces;
		_position = (_bouncing ? _position : groundHeight);
	}

	private void OnEnable()
	{
		if (resetOnEnable)
		{
			OnValueChanged.Invoke(startingHeight);
		}
		if (bounceOnEnable)
		{
			Bounce();
		}
	}

	private void Update()
	{
		if (_bouncing)
		{
			float num = GameUtil.GetDeltaTime(useScaledTime);
			while (num > 0f && _bouncing)
			{
				float num2 = Mathf.Min(num, 1f / 60f);
				_UpdateStep(num2);
				num -= num2;
			}
			OnValueChanged.Invoke(_position);
		}
	}

	public void Bounce()
	{
		_bouncing = true;
		_position = startingHeight;
		_velocity = _initialVelocity;
		_bounciness = _initialBounciness;
		_bounceCount = 0;
		Update();
	}
}
