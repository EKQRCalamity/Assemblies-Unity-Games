using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Condition;

[ActionCategory("Blasphemous Condition")]
[Tooltip("Checks if the altar tier is greater or equal to a given tier.")]
public class IsAltarTierGreaterOrEqual : FsmStateAction
{
	[RequiredField]
	public FsmInt tierToCheck;

	public FsmEvent currentIsGreaterOrEqual;

	public FsmEvent currentIsLower;

	public override void Reset()
	{
		if (tierToCheck == null)
		{
			tierToCheck = new FsmInt();
		}
		tierToCheck.Value = 0;
	}

	public override void OnEnter()
	{
		int num = ((tierToCheck != null) ? tierToCheck.Value : 0);
		if (Core.Alms.GetAltarLevel() >= num)
		{
			base.Fsm.Event(currentIsGreaterOrEqual);
		}
		else
		{
			base.Fsm.Event(currentIsLower);
		}
		Finish();
	}
}
