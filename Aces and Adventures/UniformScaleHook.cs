using UnityEngine;

public class UniformScaleHook : MonoBehaviour
{
	public void UniformScale(float scale)
	{
		base.gameObject.transform.localScale = new Vector3(scale, scale, scale);
	}

	public void ScaleXZ(float scale)
	{
		base.gameObject.transform.localScale = new Vector3(scale, base.gameObject.transform.localScale.y, scale);
	}
}
