using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupHook : MonoBehaviour
{
	public bool deactivateChildrenOnly = true;

	private CanvasGroup _canvasGroup;

	public CanvasGroup canvasGroup
	{
		get
		{
			if (!(_canvasGroup != null))
			{
				return _canvasGroup = GetComponent<CanvasGroup>();
			}
			return _canvasGroup;
		}
	}

	public float alphaSquared
	{
		get
		{
			return canvasGroup.alpha;
		}
		set
		{
			canvasGroup.alpha = value * value;
		}
	}

	public float alphaCubed
	{
		get
		{
			return canvasGroup.alpha;
		}
		set
		{
			canvasGroup.alpha = value * value * value;
		}
	}

	public void SetAlpha(float alpha)
	{
		bool flag = canvasGroup.alpha > 0f;
		canvasGroup.alpha = alpha;
		bool flag2 = canvasGroup.alpha > 0f;
		if (flag2 == flag)
		{
			return;
		}
		if (!deactivateChildrenOnly)
		{
			base.gameObject.SetActive(flag2);
			return;
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(flag2);
		}
	}

	public void SetColor(Color color)
	{
		SetAlpha(color.a);
	}
}
