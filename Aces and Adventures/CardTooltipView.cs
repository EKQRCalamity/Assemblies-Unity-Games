using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CardTooltipView : MonoBehaviour
{
	public RectTransform iconContainer;

	private TextMeshProUGUI _text;

	public TextMeshProUGUI text => this.CacheComponentInChildren(ref _text);

	public CardTooltipView SetText(string textToSet)
	{
		text.text = textToSet;
		return this;
	}

	public CardTooltipView SetIcon(GameObject icon)
	{
		iconContainer.gameObject.DestroyChildren(immediate: false);
		icon.transform.SetParent(iconContainer, worldPositionStays: false);
		return this;
	}
}
