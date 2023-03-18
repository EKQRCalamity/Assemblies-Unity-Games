using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Returns TRUE if the player has neither prayers, relics, sword heart nor rosary beads equiped.")]
public class NoItemEquiped : FsmStateAction
{
	public FsmEvent onSuccess;

	public FsmEvent onFailure;

	public override void OnEnter()
	{
		bool flag = true;
		if (Core.InventoryManager.IsAnyPrayerEquipped())
		{
			flag = false;
		}
		else if (Core.InventoryManager.IsAnyRelicEquipped())
		{
			flag = false;
		}
		else if (Core.InventoryManager.IsAnySwordHeartEquiped())
		{
			flag = false;
		}
		else if (Core.InventoryManager.IsAnyRosaryBeadEquiped())
		{
			flag = false;
		}
		if (flag && onSuccess != null)
		{
			base.Fsm.Event(onSuccess);
		}
		if (!flag && onFailure != null)
		{
			base.Fsm.Event(onFailure);
		}
	}
}
