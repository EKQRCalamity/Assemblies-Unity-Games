using System;
using Framework.Inventory;
using Framework.Managers;

[Serializable]
public class InventoryObjectInspector
{
	public InventoryManager.ItemType type = InventoryManager.ItemType.Quest;

	public string id = string.Empty;

	public BaseInventoryObject GetInvObject()
	{
		BaseInventoryObject result = null;
		switch (type)
		{
		case InventoryManager.ItemType.Bead:
			result = Core.InventoryManager.GetRosaryBead(id);
			break;
		case InventoryManager.ItemType.Collectible:
			result = Core.InventoryManager.GetCollectibleItem(id);
			break;
		case InventoryManager.ItemType.Prayer:
			result = Core.InventoryManager.GetPrayer(id);
			break;
		case InventoryManager.ItemType.Quest:
			result = Core.InventoryManager.GetQuestItem(id);
			break;
		case InventoryManager.ItemType.Relic:
			result = Core.InventoryManager.GetRelic(id);
			break;
		case InventoryManager.ItemType.Sword:
			result = Core.InventoryManager.GetSword(id);
			break;
		}
		return result;
	}
}
