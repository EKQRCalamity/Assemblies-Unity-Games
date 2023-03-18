using Framework.Map;
using UnityEngine;
using UnityEngine.UI;

public class NewMapMenuWidgetMarkItem : MonoBehaviour
{
	public string IconChildName = "Icon";

	public string SelectionChildName = "Selection";

	private Image Sprite;

	private GameObject Selection;

	private Image SelectionImage;

	public MapData.MarkType MarkId { get; private set; }

	public void SetInitialData(MapData.MarkType id, Sprite sprite, bool selected)
	{
		Sprite = base.transform.Find(IconChildName).GetComponent<Image>();
		Selection = base.transform.Find(SelectionChildName).gameObject;
		SelectionImage = Selection.GetComponent<Image>();
		RectTransform rectTransform = (RectTransform)Sprite.transform;
		rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
		Sprite.sprite = sprite;
		SetSelected(selected);
		MarkId = id;
	}

	public void SetSelected(bool selected)
	{
		Selection.SetActive(selected);
	}

	public void SetDisabled(bool disabled)
	{
		SelectionImage.color = ((!disabled) ? Color.white : Color.gray);
		Sprite.color = ((!disabled) ? Color.white : Color.gray);
	}
}
