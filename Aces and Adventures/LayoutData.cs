using UnityEngine;
using UnityEngine.UI;

public struct LayoutData
{
	public float min;

	public float preferred;

	public float flexible;

	public float this[ContentSizeFitter.FitMode fit] => fit switch
	{
		ContentSizeFitter.FitMode.MinSize => min, 
		ContentSizeFitter.FitMode.PreferredSize => preferred, 
		ContentSizeFitter.FitMode.Unconstrained => -1f, 
		_ => preferred, 
	};

	public LayoutData(float min, float preferred, float flexible)
	{
		this.min = min;
		this.preferred = preferred;
		this.flexible = flexible;
	}

	public float GetSize(RectTransform rect, ContentSizeFitter.FitMode fit, int axis)
	{
		float num = this[fit];
		if (num < 0f)
		{
			return rect.rect.size[axis];
		}
		return num;
	}

	public bool Clamp(float min, float max = 0f)
	{
		float num = this.min;
		float num2 = preferred;
		max = Mathf.Max(min, max);
		this.min = Mathf.Max(min, this.min);
		preferred = Mathf.Max(min, preferred);
		if (max > 0f)
		{
			this.min = Mathf.Min(max, this.min);
			preferred = Mathf.Min(max, preferred);
		}
		if (num == this.min)
		{
			return num2 != preferred;
		}
		return true;
	}

	public static LayoutData operator -(LayoutData a, LayoutData b)
	{
		return new LayoutData(a.min - b.min, a.preferred - b.preferred, a.flexible - b.flexible);
	}
}
