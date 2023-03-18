using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Shows a message indicating that the chosen item has been added to the player inventory. This action DOES NOT adds the item, it will only show a message")]
public class ItemAdditionMessage : InventoryBase
{
	public override bool executeAction(string objectIdStting, InventoryManager.ItemType objType, int slot)
	{
		BaseInventoryObject baseObjectOrTears = Core.InventoryManager.GetBaseObjectOrTears(objectIdStting, objType);
		UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, baseObjectOrTears.caption, baseObjectOrTears.picture, objType, 3f, blockPlayer: true);
		return true;
	}

	public override void OnEnter()
	{
		PopUpWidget.OnDialogClose += DialogClose;
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
