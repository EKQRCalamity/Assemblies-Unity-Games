using UnityEngine;

public class RotationHook : MonoBehaviour
{
	public void XRotation(float degrees)
	{
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		base.transform.localRotation = Quaternion.Euler(degrees, eulerAngles.y, eulerAngles.z);
	}

	public void YRotation(float degrees)
	{
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		base.transform.localRotation = Quaternion.Euler(eulerAngles.x, degrees, eulerAngles.z);
	}

	public void ZRotation(float degrees)
	{
		Vector3 eulerAngles = base.transform.localRotation.eulerAngles;
		base.transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, degrees);
	}
}
