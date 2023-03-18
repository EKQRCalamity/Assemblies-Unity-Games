using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public class InventoryIdAttribute : PropertyAttribute
{
	public InventoryManager.ItemType type;

	public InventoryIdAttribute(InventoryManager.ItemType type)
	{
		this.type = type;
	}
}
