using UnityEngine;

[DisallowMultipleComponent]
public class AnimatePosition : AnimateTransform
{
	public override void CacheInitialValues()
	{
		initialVector = base.transform.localPosition;
	}

	protected override void UniqueUpdate(float t)
	{
		base.transform.localPosition = GetVector(base.transform.localPosition, t);
	}
}
