using Framework.Managers;
using Gameplay.UI;
using UnityEngine;

namespace Framework.Inventory;

public class InteractableInvAdd : MonoBehaviour
{
	public InventoryManager.ItemType itemType;

	public string item = string.Empty;

	public bool showMessage = true;

	private void OnUsePost()
	{
		BaseInventoryObject baseObject = Core.InventoryManager.GetBaseObject(item, itemType);
		baseObject = Core.InventoryManager.AddBaseObjectOrTears(baseObject);
		if ((bool)baseObject)
		{
			Core.Persistence.SaveGame();
			if (showMessage)
			{
				UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, baseObject.caption, baseObject.picture, itemType);
			}
		}
	}
}
