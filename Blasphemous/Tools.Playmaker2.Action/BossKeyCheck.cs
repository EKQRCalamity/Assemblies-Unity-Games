using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Add or remove purge points.")]
public class BossKeyCheck : FsmStateAction
{
	public FsmInt slot;

	public FsmBool outValue;

	public FsmEvent bossKeyFound;

	public FsmEvent bossKeyNotFound;

	public override void Reset()
	{
		outValue = new FsmBool
		{
			UseVariable = true
		};
	}

	public override void OnEnter()
	{
		int num = ((slot != null) ? slot.Value : 0);
		bool flag = Core.InventoryManager.CheckBossKey(num);
		if (flag)
		{
			base.Fsm.Event(bossKeyFound);
		}
		else
		{
			base.Fsm.Event(bossKeyNotFound);
		}
		if (outValue != null)
		{
			outValue.Value = flag;
		}
		Finish();
	}
}
