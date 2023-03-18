using Framework.Inventory;
using Framework.Managers;
using Gameplay.UI;
using Gameplay.UI.Others.MenuLogic;
using HutongGames.PlayMaker;

namespace Tools.PlayMaker.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Add or remove purge points.")]
public class TearsAddition : FsmStateAction
{
	public FsmFloat Tears;

	public FsmBool ShowMessage;

	public FsmEvent onSuccess;

	public override void OnEnter()
	{
		float num = ((Tears == null) ? 0f : Tears.Value);
		bool flag = ShowMessage != null && ShowMessage.Value;
		float num2 = Core.Logic.Penitent.Stats.Purge.Current + num;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		Core.Logic.Penitent.Stats.Purge.Current = num2;
		if (flag)
		{
			PopUpWidget.OnDialogClose += DialogClose;
			TearsObject tearsGenericObject = Core.InventoryManager.TearsGenericObject;
			UIController.instance.ShowObjectPopUp(UIController.PopupItemAction.GetObejct, tearsGenericObject.caption, tearsGenericObject.picture, tearsGenericObject.GetItemType(), 3f, blockPlayer: true);
			return;
		}
		if (onSuccess != null)
		{
			base.Fsm.Event(onSuccess);
		}
		Finish();
	}

	private void DialogClose()
	{
		PopUpWidget.OnDialogClose -= DialogClose;
		if (onSuccess != null)
		{
			base.Fsm.Event(onSuccess);
		}
		Finish();
	}
}
