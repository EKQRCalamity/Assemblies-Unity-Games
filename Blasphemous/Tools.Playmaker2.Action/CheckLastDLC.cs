using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Checks the last DLC installed")]
public class CheckLastDLC : FsmStateAction
{
	public FsmEvent lastDLCIsOne;

	public FsmEvent lastDLCIsTwo;

	public FsmEvent lastDLCIsThree;

	public override void OnEnter()
	{
		if (Core.DLCManager.IsThirdDLCInstalled())
		{
			base.Fsm.Event(lastDLCIsThree);
		}
		else if (Core.DLCManager.IsSecondDLCInstalled())
		{
			base.Fsm.Event(lastDLCIsTwo);
		}
		else if (Core.DLCManager.IsFirstDLCInstalled())
		{
			base.Fsm.Event(lastDLCIsOne);
		}
		Finish();
	}
}
