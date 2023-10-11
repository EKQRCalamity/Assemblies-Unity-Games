using UnityEngine;

public class TransformCurves : ACurves
{
	[Header("Transform=======================================================================")]
	public Vector3Curve localPosition;

	public Vector3Curve localRotation;

	public Vector3Curve localScale;

	private Transform _t;

	protected Transform t
	{
		get
		{
			if (!(_t != null))
			{
				return _InitializeValues();
			}
			return _t;
		}
	}

	private Transform _InitializeValues()
	{
		_t = base.transform;
		localPosition.initialValue = base.transform.localPosition;
		localRotation.initialValue = base.transform.localRotation.eulerAngles;
		localScale.initialValue = base.transform.localScale;
		return _t;
	}

	private void Awake()
	{
		_ = t;
	}

	protected override void _Input(float t)
	{
		if (localPosition.enabled)
		{
			this.t.localPosition = localPosition.GetValue(t);
		}
		if (localRotation.enabled)
		{
			this.t.localRotation = Quaternion.Euler(localRotation.GetValue(t));
		}
		if (localScale.enabled)
		{
			this.t.localScale = localScale.GetValue(t);
		}
	}
}
