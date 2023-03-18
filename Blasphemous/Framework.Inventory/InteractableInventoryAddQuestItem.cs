using Framework.Managers;
using Gameplay.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.Inventory;

public class InteractableInventoryAddQuestItem : MonoBehaviour
{
	[InventoryId(InventoryManager.ItemType.Quest)]
	public string questItem;

	[InfoBox("Este componente está deprecado!! Usa InteractableInvAdd.", InfoMessageType.Warning, null)]
	public bool showMessage = true;

	public string sound = "event:/Key Event/Quest Item";

	private void OnUsePost()
	{
		BaseInventoryObject baseInventoryObject = Core.InventoryManager.GetQuestItem(questItem);
		if (!baseInventoryObject)
		{
			return;
		}
		baseInventoryObject = Core.InventoryManager.AddBaseObjectOrTears(baseInventoryObject);
		if ((bool)baseInventoryObject)
		{
			Core.Persistence.SaveGame();
			if (showMessage)
			{
				UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, baseInventoryObject.caption, baseInventoryObject.picture, InventoryManager.ItemType.Quest);
			}
		}
	}
}
