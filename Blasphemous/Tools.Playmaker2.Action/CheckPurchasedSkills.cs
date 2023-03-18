using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
public class CheckPurchasedSkills : FsmStateAction
{
	public FsmEvent notPurchased;

	public FsmEvent allPurchased;

	public override void OnUpdate()
	{
		int lockedSkillsNumber = Core.SkillManager.GetLockedSkillsNumber();
		if (lockedSkillsNumber == 0)
		{
			base.Fsm.Event(allPurchased);
		}
		if (lockedSkillsNumber > 0)
		{
			base.Fsm.Event(notPurchased);
		}
		Finish();
	}
}
