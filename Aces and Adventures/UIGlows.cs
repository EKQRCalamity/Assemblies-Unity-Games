using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(ColorRelay))]
public class UIGlows : MonoBehaviour
{
	private static GameObject _Blueprint;

	private static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("UI/UIGlows");
			}
			return _Blueprint;
		}
	}

	public static UIGlows Create(Image image)
	{
		int siblingIndex = image.transform.GetSiblingIndex();
		GameObject obj = Pools.Unpool(Blueprint, image.transform.parent);
		obj.transform.SetSiblingIndex(siblingIndex);
		UIGlows uIGlows = obj.GetComponent<UIGlows>().SetImage(image);
		(uIGlows.transform as RectTransform).CopyRect(image.transform as RectTransform);
		return uIGlows;
	}

	public void SetSprite(Sprite sprite)
	{
		UIGlow[] componentsInChildren = GetComponentsInChildren<UIGlow>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sprite = sprite;
		}
	}

	public UIGlows SetImage(Image image)
	{
		UIGlow[] componentsInChildren = GetComponentsInChildren<UIGlow>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].sprite = image.sprite;
		}
		return this;
	}

	public UIGlows SetColor(Color32 glowColor)
	{
		GetComponent<ColorRelay>().ChangeColor(glowColor);
		return this;
	}

	public UIGlows SetAlpha(float alpha)
	{
		GetComponent<ColorRelay>().ChangeColor(GetComponent<ColorRelay>().color.SetAlpha(alpha));
		return this;
	}
}
