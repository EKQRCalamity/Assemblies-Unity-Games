using Gameplay.UI;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if the inventory is showing.")]
public class IsShowingInventory : FsmStateAction
{
	public FsmEvent isShowing;

	public FsmEvent notShowing;

	public override void OnEnter()
	{
		if (UIController.instance.IsShowingInventory)
		{
			base.Fsm.Event(isShowing);
		}
		else
		{
			base.Fsm.Event(notShowing);
		}
		Finish();
	}
}
