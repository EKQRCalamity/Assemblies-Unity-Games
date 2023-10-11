using UnityEngine;

public class PositionHook : MonoBehaviour
{
	private RectTransform _rect;

	public RectTransform rect
	{
		get
		{
			if (!(_rect != null))
			{
				return _rect = base.transform as RectTransform;
			}
			return _rect;
		}
	}

	public float x
	{
		get
		{
			return base.transform.position.x;
		}
		set
		{
			base.transform.position = new Vector3(value, base.transform.position.y, base.transform.position.z);
		}
	}

	public float y
	{
		get
		{
			return base.transform.position.y;
		}
		set
		{
			base.transform.position = new Vector3(base.transform.position.x, value, base.transform.position.z);
		}
	}

	public float z
	{
		get
		{
			return base.transform.position.z;
		}
		set
		{
			base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, value);
		}
	}

	public float xLocal
	{
		get
		{
			return base.transform.localPosition.x;
		}
		set
		{
			base.transform.localPosition = new Vector3(value, base.transform.localPosition.y, base.transform.localPosition.z);
		}
	}

	public float yLocal
	{
		get
		{
			return base.transform.localPosition.y;
		}
		set
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, value, base.transform.localPosition.z);
		}
	}

	public float zLocal
	{
		get
		{
			return base.transform.localPosition.z;
		}
		set
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, value);
		}
	}

	public float xPivot
	{
		get
		{
			return rect.pivot.x;
		}
		set
		{
			rect.pivot = new Vector2(value, rect.pivot.y);
		}
	}

	public float yPivot
	{
		get
		{
			return rect.pivot.y;
		}
		set
		{
			rect.pivot = new Vector2(rect.pivot.x, value);
		}
	}

	public float xAnchors
	{
		set
		{
			rect.anchorMin = new Vector2(value, rect.anchorMin.y);
			rect.anchorMax = new Vector2(value, rect.anchorMax.y);
		}
	}

	public float yAnchors
	{
		set
		{
			rect.anchorMin = new Vector2(rect.anchorMin.x, value);
			rect.anchorMax = new Vector2(rect.anchorMax.x, value);
		}
	}

	public float xAnchorsShifted
	{
		set
		{
			rect.anchorMin = new Vector2(value, rect.anchorMin.y);
			rect.anchorMax = new Vector2(value + 1f, rect.anchorMax.y);
		}
	}

	public float yAnchorsShifted
	{
		set
		{
			rect.anchorMin = new Vector2(rect.anchorMin.x, value);
			rect.anchorMax = new Vector2(rect.anchorMax.x, value + 1f);
		}
	}
}
