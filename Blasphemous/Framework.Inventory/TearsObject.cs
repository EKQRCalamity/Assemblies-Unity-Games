using Framework.Managers;

namespace Framework.Inventory;

public class TearsObject : BaseInventoryObject
{
	public float TearsForDuplicatedObject = 1200f;

	public override InventoryManager.ItemType GetItemType()
	{
		return InventoryManager.ItemType.Quest;
	}
}
