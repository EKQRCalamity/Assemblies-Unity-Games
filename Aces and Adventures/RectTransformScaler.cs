using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectTransformScaler : MonoBehaviour
{
	[SerializeField]
	[Range(0.1f, 10f)]
	protected float _scale = 1f;

	[SerializeField]
	protected bool _xAnchor = true;

	[SerializeField]
	protected bool _yAnchor;

	public float scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _scale, value))
			{
				_Update();
			}
		}
	}

	public bool xAnchor
	{
		get
		{
			return _xAnchor;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _xAnchor, value))
			{
				_Update();
			}
		}
	}

	public bool yAnchor
	{
		get
		{
			return _yAnchor;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref _yAnchor, value))
			{
				_Update();
			}
		}
	}

	private void _Update()
	{
		base.transform.localScale = new Vector3(scale, scale, scale);
		RectTransform obj = base.transform as RectTransform;
		obj.anchorMin = Vector2.zero;
		float num = 1f / scale;
		obj.anchorMax = new Vector2(xAnchor ? num : 1f, yAnchor ? num : 1f);
	}
}
