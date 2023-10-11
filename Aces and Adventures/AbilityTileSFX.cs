using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class AbilityTileSFX : MonoBehaviour
{
	public const float NO_SENTIENT_WAIT = 0.2f;

	public const float MAX_WAIT = 5f;

	private const float LIFETIME = 10f;

	[Header("Rotation")]
	public AbilityTileSFXOrientType orientation;

	public CardinalOrientation relativeRotation;

	[Range(0f, 180f)]
	public float rotationSnapDegrees = 45f;

	[Header("Events")]
	[Range(0.001f, 5f)]
	public float actEventTimestamp = 0.5f;

	public UnityEvent OnAct;

	private float _elapsedTime;

	private void OnDisable()
	{
		if ((bool)this)
		{
			OnAct.RemoveAllListeners();
		}
	}

	private void Update()
	{
		float elapsedTime = _elapsedTime;
		_elapsedTime += Time.deltaTime;
		if (elapsedTime < actEventTimestamp && _elapsedTime >= actEventTimestamp)
		{
			Act();
		}
		if (_elapsedTime >= 10f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public Quaternion GetRotation(Short2 position, PositionOrientPair activationOriginPositionOrient, PositionOrientPair activatorPositionOrient, PositionOrientPair? targetPositionOrient, System.Random random)
	{
		Quaternion quaternion = relativeRotation.Rotate(orientation.GetRotation(position, activationOriginPositionOrient, activatorPositionOrient, targetPositionOrient, random));
		return (rotationSnapDegrees > 0f) ? Quaternion.Euler(quaternion.eulerAngles.RoundAxisToNearestMultipleOf(AxisType.Y, rotationSnapDegrees)) : quaternion;
	}

	public void Act()
	{
		OnAct.Invoke();
	}
}
