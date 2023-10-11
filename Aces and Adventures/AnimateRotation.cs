using UnityEngine;

[DisallowMultipleComponent]
public class AnimateRotation : AnimateTransform
{
	[Header("Rotation Space")]
	public bool useLocalRotation = true;

	public override void CacheInitialValues()
	{
		initialVector = (useLocalRotation ? base.transform.localRotation.eulerAngles : base.transform.rotation.eulerAngles);
	}

	protected override void UniqueUpdate(float t)
	{
		if (useLocalRotation)
		{
			base.transform.localRotation = Quaternion.Euler(GetVector(base.transform.localRotation.eulerAngles, t, useLateUpdate));
		}
		else
		{
			base.transform.rotation = Quaternion.Euler(GetVector(base.transform.rotation.eulerAngles, t, useLateUpdate));
		}
	}
}
