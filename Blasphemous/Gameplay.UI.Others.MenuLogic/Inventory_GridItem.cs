using Framework.Inventory;
using Gameplay.UI.Others.Buttons;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class Inventory_GridItem : MonoBehaviour
{
	[SerializeField]
	private GameObject frameObj;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Texture emptyFrame;

	[SerializeField]
	private Sprite emptyItem;

	[SerializeField]
	private Texture objectFrame;

	[SerializeField]
	private Texture equipedFrame;

	[SerializeField]
	private Texture selectedFrame;

	[SerializeField]
	private Texture selectedFrameEmpty;

	private RawImage frameImage;

	public EventsButton Button { get; private set; }

	public BaseInventoryObject inventoryObject { get; private set; }

	public void SetObject(BaseInventoryObject invObj)
	{
		inventoryObject = invObj;
		Button = frameObj.GetComponent<EventsButton>();
		frameImage = frameObj.GetComponent<RawImage>();
		Sprite picture = emptyItem;
		if ((bool)invObj)
		{
			picture = invObj.picture;
		}
		image.sprite = picture;
	}

	public void UpdateStatus(bool selected, bool equiped)
	{
		Texture texture = emptyFrame;
		if (selected)
		{
			texture = selectedFrameEmpty;
			if ((bool)inventoryObject)
			{
				texture = selectedFrame;
			}
		}
		else if ((bool)inventoryObject)
		{
			texture = ((!equiped) ? objectFrame : equipedFrame);
		}
		frameImage.texture = texture;
	}
}
