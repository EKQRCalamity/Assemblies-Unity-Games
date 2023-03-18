using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Checks if an Achievement is granted.")]
public class CheckAchievement : FsmStateAction
{
	[RequiredField]
	public FsmString achievementId;

	public FsmEvent achievementGranted;

	public FsmEvent achievementNotGranted;

	public override void OnEnter()
	{
		if (Core.AchievementsManager.CheckAchievementGranted(achievementId.Value))
		{
			base.Fsm.Event(achievementGranted);
		}
		else
		{
			base.Fsm.Event(achievementNotGranted);
		}
		Finish();
	}
}
