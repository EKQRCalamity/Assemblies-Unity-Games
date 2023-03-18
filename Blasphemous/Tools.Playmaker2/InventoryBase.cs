using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2;

public abstract class InventoryBase : FsmStateAction
{
	[Tooltip("Type of object")]
	public FsmInt itemType;

	[Tooltip("Object ID")]
	public FsmString objectId;

	[Tooltip("Slot")]
	public FsmInt slot;

	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public virtual bool UseObject => true;

	public virtual bool UseSlot => false;

	public override void Reset()
	{
		if (objectId != null)
		{
			objectId.Value = string.Empty;
		}
	}

	public override void OnEnter()
	{
		string text = ((objectId == null) ? string.Empty : objectId.Value);
		int objType = ((itemType != null) ? itemType.Value : 0);
		int num = ((slot != null) ? slot.Value : 0);
		if (UseObject && string.IsNullOrEmpty(text))
		{
			LogWarning("PlayMaker Inventory Action - objectId is blank");
			return;
		}
		bool flag = executeAction(text, (InventoryManager.ItemType)objType, num);
		if (flag && onSuccess != null)
		{
			base.Fsm.Event(onSuccess);
		}
		if (!flag && onFailure != null)
		{
			base.Fsm.Event(onFailure);
		}
		Finish();
	}

	public abstract bool executeAction(string objectIdStting, InventoryManager.ItemType objType, int slot);
}
