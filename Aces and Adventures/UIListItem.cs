using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIListItem : MonoBehaviour
{
	public TextMeshProUGUI text;

	private Image toggleImage;

	public object value { get; private set; }

	private void Awake()
	{
		text = GetComponentInChildren<TextMeshProUGUI>();
		toggleImage = base.gameObject.GetComponentInChildrenOnly<Image>();
	}

	public void Init(UIListItemData data)
	{
		text.text = data.text;
		value = data.value;
	}

	public void SetToggleImageSprite(Sprite sprite)
	{
		if (toggleImage != null)
		{
			toggleImage.sprite = sprite;
		}
	}

	public void SetToggleImageState(bool enabled)
	{
		if (toggleImage != null)
		{
			toggleImage.enabled = enabled;
		}
	}
}
