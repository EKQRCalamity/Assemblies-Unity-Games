using Framework.Inventory;
using Framework.Managers;
using Sirenix.OdinInspector;

namespace Tools.Items;

public class ObjectEffect_ChangeItem : ObjectEffect
{
	public bool addObject;

	[ShowIf("addObject", true)]
	public InventoryObjectInspector NewItem;

	[ShowIf("addObject", true)]
	public bool equip;

	private void OnResetInventoryObject()
	{
	}

	protected override bool OnApplyEffect()
	{
		bool flag = true;
		if (addObject)
		{
			flag = Core.InventoryManager.AddBaseObject(NewItem.GetInvObject());
		}
		int baseObjectEquippedSlot = Core.InventoryManager.GetBaseObjectEquippedSlot(InvObj);
		flag = flag && Core.InventoryManager.RemoveBaseObject(InvObj);
		if (equip && baseObjectEquippedSlot != -1)
		{
			Core.InventoryManager.EquipBaseObject(NewItem.GetInvObject(), baseObjectEquippedSlot);
		}
		return flag;
	}
}
