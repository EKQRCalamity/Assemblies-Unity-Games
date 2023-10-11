using UnityEngine;

public class ScaleHook : MonoBehaviour
{
	public float xLocal
	{
		get
		{
			return base.transform.localScale.x;
		}
		set
		{
			base.transform.localScale = base.transform.localScale.SetAxis(AxisType.X, value);
		}
	}

	public float yLocal
	{
		get
		{
			return base.transform.localScale.y;
		}
		set
		{
			base.transform.localScale = base.transform.localScale.SetAxis(AxisType.Y, value);
		}
	}

	public float zLocal
	{
		get
		{
			return base.transform.localScale.z;
		}
		set
		{
			base.transform.localScale = base.transform.localScale.SetAxis(AxisType.Z, value);
		}
	}
}
