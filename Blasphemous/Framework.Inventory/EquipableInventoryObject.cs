using Framework.Managers;
using UnityEngine;

namespace Framework.Inventory;

public abstract class EquipableInventoryObject : BaseInventoryObject
{
	public bool UsePercentageCompletition = true;

	public bool IsEquiped { get; private set; }

	public override bool IsEquipable()
	{
		return true;
	}

	public override bool AskForPercentageCompletition()
	{
		return true;
	}

	public override bool AddPercentageCompletition()
	{
		return UsePercentageCompletition;
	}

	public void Equip()
	{
		IsEquiped = true;
		SendMessage("OnEquipInventoryObject", SendMessageOptions.DontRequireReceiver);
	}

	public void UnEquip()
	{
		IsEquiped = false;
		SendMessage("OnUnEquipInventoryObject", SendMessageOptions.DontRequireReceiver);
	}

	public void Use()
	{
		Core.Metrics.CustomEvent("ITEM_USED", base.name);
		Core.Metrics.HeatmapEvent("ITEM_USED", Core.Logic.Penitent.transform.position);
		SendMessage("OnUseInventoryObject", SendMessageOptions.DontRequireReceiver);
	}
}
