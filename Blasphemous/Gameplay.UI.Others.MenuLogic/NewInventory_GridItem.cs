using System.Collections.Generic;
using Framework.Inventory;
using Gameplay.UI.Others.Buttons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.MenuLogic;

public class NewInventory_GridItem : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Image frameBack;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private GameObject frameObj;

	[SerializeField]
	[BoxGroup("Controls", true, false, 0)]
	private Image objectImage;

	[SerializeField]
	[BoxGroup("Frame", true, false, 0)]
	private Sprite frameSelected;

	[SerializeField]
	[BoxGroup("Back", true, false, 0)]
	private Sprite backDisabled;

	[SerializeField]
	[BoxGroup("Back", true, false, 0)]
	private Sprite backNothing;

	[SerializeField]
	[BoxGroup("Back", true, false, 0)]
	private Sprite backUnEquipped;

	[SerializeField]
	[BoxGroup("Back", true, false, 0)]
	private Sprite backEquipped;

	private Image frameImage;

	private Animator frameAnimator;

	private List<Image> cachedImages;

	public EventsButton Button { get; private set; }

	public BaseInventoryObject inventoryObject { get; private set; }

	private void Awake()
	{
		cachedImages = new List<Image>(GetComponentsInChildren<Image>(includeInactive: true));
	}

	public void SetObject(BaseInventoryObject invObj)
	{
		inventoryObject = invObj;
		Button = frameObj.GetComponent<EventsButton>();
		frameImage = frameObj.GetComponent<Image>();
		frameAnimator = frameObj.GetComponent<Animator>();
		if ((bool)invObj)
		{
			objectImage.sprite = invObj.picture;
			objectImage.enabled = true;
		}
		else
		{
			objectImage.enabled = false;
		}
	}

	public void UpdateStatus(bool p_enabled, bool p_selected, bool p_equiped)
	{
		Sprite sprite = backDisabled;
		if (p_enabled)
		{
			sprite = backNothing;
			if ((bool)inventoryObject)
			{
				sprite = ((!p_equiped) ? backUnEquipped : backEquipped);
			}
		}
		frameBack.sprite = sprite;
		UpdateSelect(p_selected);
	}

	public void UpdateSelect(bool selected)
	{
		Sprite sprite = ((!selected) ? null : frameSelected);
		if ((bool)frameImage)
		{
			frameImage.sprite = sprite;
			frameImage.enabled = sprite != null;
		}
		if ((bool)frameAnimator)
		{
			frameAnimator.enabled = sprite != null;
		}
	}

	public void ActivateGrayscale()
	{
		foreach (Image cachedImage in cachedImages)
		{
			cachedImage.material.EnableKeyword("COLORIZE_ON");
			cachedImage.gameObject.SetActive(value: false);
			cachedImage.gameObject.SetActive(value: true);
		}
	}

	public void DeactivateGrayscale()
	{
		foreach (Image cachedImage in cachedImages)
		{
			cachedImage.material.DisableKeyword("COLORIZE_ON");
			cachedImage.gameObject.SetActive(value: false);
			cachedImage.gameObject.SetActive(value: true);
		}
	}
}
