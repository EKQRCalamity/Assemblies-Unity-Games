using UnityEngine;

public class LiquidSurfaceOrienter : MonoBehaviour
{
	public float springConstant = 100f;

	public float dampening = 10f;

	private Vector4 _rotationVelocity;

	private Quaternion _initialLocalRotation;

	private void OnEnable()
	{
		_initialLocalRotation = base.transform.localRotation;
	}

	private void LateUpdate()
	{
		Quaternion current = base.transform.rotation;
		base.transform.rotation = MathUtil.Spring(ref current, ref _rotationVelocity, Quaternion.LookRotation(Vector3.up, base.transform.parent.up), springConstant, dampening, Time.deltaTime);
	}

	private void OnDisable()
	{
		base.transform.localRotation = _initialLocalRotation;
	}
}
