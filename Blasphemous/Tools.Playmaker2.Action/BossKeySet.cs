using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Add or remove purge points.")]
public class BossKeySet : FsmStateAction
{
	public FsmInt slot;

	public FsmBool value;

	public override void OnEnter()
	{
		int num = ((slot != null) ? slot.Value : 0);
		if (value == null || value.Value)
		{
			Core.InventoryManager.AddBossKey(num);
		}
		else
		{
			Core.InventoryManager.RemoveBossKey(num);
		}
		Finish();
	}
}
