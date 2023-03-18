using Framework.Inventory;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Unequip the chosen item.")]
public class ItemUnequip : InventoryBase
{
	public override bool UseSlot => true;

	public override bool UseObject => false;

	public override bool executeAction(string objectIdStting, InventoryManager.ItemType objType, int slot)
	{
		bool result = false;
		switch (objType)
		{
		case InventoryManager.ItemType.Bead:
			result = Core.InventoryManager.SetRosaryBeadInSlot(slot, (RosaryBead)null);
			break;
		case InventoryManager.ItemType.Prayer:
			result = Core.InventoryManager.SetPrayerInSlot(slot, (Prayer)null);
			break;
		case InventoryManager.ItemType.Relic:
			result = Core.InventoryManager.SetRelicInSlot(slot, (Relic)null);
			break;
		case InventoryManager.ItemType.Sword:
			result = Core.InventoryManager.SetSwordInSlot(slot, (Sword)null);
			break;
		}
		return result;
	}
}
