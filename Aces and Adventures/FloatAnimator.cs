using System;
using UnityEngine;

[Serializable]
public class FloatAnimator
{
	public enum AnimationType
	{
		Constant,
		Ease,
		Spring,
		Immediate
	}

	private const float SNAP_DISTANCE = 0.001f;

	private const float SNAP_VELOCITY = 0.01f;

	private static readonly Vector2 EASE_RANGE = new Vector2(1f, 20f);

	private static readonly Vector2 CONSTANT_RANGE = new Vector2(0f, 10f);

	private static readonly Vector2 SPRING_RANGE = new Vector2(10f, 200f);

	private static readonly Vector2 SPRING_DAMPEN_RANGE = new Vector2(0f, 10f);

	public AnimationType animationType = AnimationType.Ease;

	[Range(0f, 1f)]
	public float speed = 0.5f;

	[Range(0f, 1f)]
	public float dampening = 0.5f;

	public bool useScaledTime = true;

	public bool allowSpringBelowZero;

	private float _position;

	private float _velocity;

	public float? targetPosition { get; set; }

	public float position => _position;

	public bool isFinished => !targetPosition.HasValue;

	private bool _IsFinished()
	{
		if (!targetPosition.HasValue)
		{
			return true;
		}
		float value = targetPosition.Value;
		switch (animationType)
		{
		case AnimationType.Constant:
		case AnimationType.Ease:
			return MathUtil.DeltaSnap(ref _position, value, 0.001f);
		case AnimationType.Spring:
			if (allowSpringBelowZero || !(value <= 0f) || !(_position <= 0f))
			{
				if (Mathf.Abs(_velocity) <= 0.01f)
				{
					return MathUtil.DeltaSnap(ref _position, value, 0.001f);
				}
				return false;
			}
			return true;
		case AnimationType.Immediate:
			_position = value;
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void Reset(int? resetPositionTo = null)
	{
		if (resetPositionTo.HasValue)
		{
			_position = resetPositionTo.Value;
		}
		_velocity = 0f;
		targetPosition = null;
	}

	public float Update()
	{
		if (!targetPosition.HasValue)
		{
			return _position;
		}
		float deltaTime = GameUtil.GetDeltaTime(useScaledTime);
		float value = targetPosition.Value;
		switch (animationType)
		{
		case AnimationType.Constant:
			MathUtil.MoveToward(ref _position, value, CONSTANT_RANGE.Lerp(speed), deltaTime);
			break;
		case AnimationType.Ease:
			MathUtil.EaseSnap(ref _position, value, EASE_RANGE.Lerp(speed), deltaTime, 0.001f);
			break;
		case AnimationType.Spring:
			MathUtil.Spring(ref _position, ref _velocity, value, SPRING_RANGE.Lerp(speed), SPRING_DAMPEN_RANGE.Lerp(dampening), deltaTime);
			break;
		case AnimationType.Immediate:
			_position = value;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		if (_IsFinished())
		{
			_position = value;
			targetPosition = null;
		}
		return _position;
	}

	public float Update(float value, float targetValue)
	{
		_position = value;
		targetPosition = targetValue;
		return Update();
	}

	public bool IsFinished(float atValue)
	{
		if (isFinished)
		{
			return _position == atValue;
		}
		return false;
	}

	public static implicit operator float(FloatAnimator data)
	{
		return data._position;
	}
}
