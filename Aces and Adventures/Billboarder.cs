using UnityEngine;

[DisallowMultipleComponent]
public class Billboarder : MonoBehaviour
{
	public Transform target;

	public AxisType facingAxis = AxisType.Z;

	public BillboardType billboardType;

	private void Start()
	{
		target = (target ? target : CameraManager.Instance.mainCamera.transform);
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = target.position;
		switch (billboardType)
		{
		case BillboardType.Vertical:
			position.x = position2.x;
			position.z = position2.z;
			break;
		case BillboardType.Horizontal:
			position.y = position2.y;
			break;
		}
		if (position2 - position != Vector3.zero)
		{
			Vector3 upwards = target.up;
			if (billboardType == BillboardType.Horizontal)
			{
				upwards = Vector3.up;
			}
			else if (billboardType == BillboardType.Vertical)
			{
				upwards = position2 - base.transform.position;
			}
			base.transform.rotation = Quaternion.LookRotation(position2 - position, upwards);
			if (facingAxis != AxisType.Z)
			{
				base.transform.rotation *= Quaternion.FromToRotation(MathUtil.GetAxis(facingAxis), Vector3.forward);
			}
		}
	}

	public void Target(Transform target)
	{
		this.target = target;
	}
}
