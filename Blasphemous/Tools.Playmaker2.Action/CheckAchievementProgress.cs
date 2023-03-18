using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Checks the percentage of progress (from 0f to 100f) of an Achievement.")]
public class CheckAchievementProgress : FsmStateAction
{
	[RequiredField]
	public FsmString achievementId;

	[UIHint(UIHint.Variable)]
	public FsmFloat achievementProgress;

	public override void OnEnter()
	{
		achievementProgress.Value = Core.AchievementsManager.CheckAchievementProgress(achievementId.Value);
		Finish();
	}
}
