using Framework.Managers;

namespace Framework.Inventory;

public class Sword : EquipableInventoryObject
{
	public static class Id
	{
		public const string SteamingIncenseHeart = "HE01";
	}

	public override InventoryManager.ItemType GetItemType()
	{
		return InventoryManager.ItemType.Sword;
	}
}
