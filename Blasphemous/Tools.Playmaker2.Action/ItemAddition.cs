using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[HutongGames.PlayMaker.Tooltip("Adds the chosen item to the player inventory.")]
public class ItemAddition : InventoryBase
{
	public FsmBool showMessage;

	public override bool executeAction(string objectIdStting, InventoryManager.ItemType objType, int slot)
	{
		BaseInventoryObject baseObject = Core.InventoryManager.GetBaseObject(objectIdStting, objType);
		if ((bool)baseObject)
		{
			baseObject = Core.InventoryManager.AddBaseObjectOrTears(baseObject);
			if (showMessage != null && showMessage.Value)
			{
				UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, baseObject.caption, baseObject.picture, objType, 3f, blockPlayer: true);
			}
			return true;
		}
		Debug.LogError("Playmaker ItemAdition Error. object " + objectIdStting + " with type " + objType.ToString() + " not found");
		base.Fsm.Event(onSuccess);
		Finish();
		return false;
	}

	public override void OnEnter()
	{
		PopUpWidget.OnDialogClose += DialogClose;
		if (!showMessage.Value)
		{
			base.OnEnter();
			return;
		}
		string text = ((objectId == null) ? string.Empty : objectId.Value);
		int objType = ((itemType != null) ? itemType.Value : 0);
		if (string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Inventory Action - objectId is blank");
		}
		else if (!executeAction(text, (InventoryManager.ItemType)objType, 0) && onFailure != null)
		{
			base.Fsm.Event(onFailure);
			Finish();
		}
	}

	public override void OnExit()
	{
		PopUpWidget.OnDialogClose -= DialogClose;
	}

	private void DialogClose()
	{
		base.Fsm.Event(onSuccess);
		Finish();
	}
}
