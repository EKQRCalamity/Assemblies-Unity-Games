using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class RigidBodyLauncher : MonoBehaviour
{
	private static System.Random _random;

	public ForceMode forceMode = ForceMode.Impulse;

	public Vector2 forceRange = new Vector2(2f, 10f);

	public float launchConeAngleDegrees = 15f;

	public Vector2 angularForceRange = new Vector2(180f, 1800f);

	public float angularForceConeAngle = 15f;

	private static System.Random random => _random ?? (_random = new System.Random());

	public void Launch(PointerEventData eventData)
	{
		Rigidbody componentInChildren = (eventData.pointerPress ? eventData.pointerPress : eventData.pointerDrag).GetComponentInChildren<Rigidbody>();
		if ((bool)componentInChildren)
		{
			float t = Mathf.Clamp01(eventData.ClickHeldTime() / InputManager.I.ClickThreshold);
			Vector3 vector = random.RandomInCone(base.transform.forward, launchConeAngleDegrees);
			componentInChildren.AddForce(vector * forceRange.Lerp(t), forceMode);
			componentInChildren.AddTorque(Quaternion.AngleAxis(MathF.PI / 180f * Time.fixedDeltaTime, random.RandomInCone(Vector3.Cross(vector, base.transform.up).normalized, angularForceConeAngle)).eulerAngles * (0f - angularForceRange.Lerp(t)), forceMode);
		}
	}
}
