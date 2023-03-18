using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Adds a percentage of progress (from 0f to 100f) to an Achievement, granting it if reaches 100f.")]
public class AddAchievementProgress : FsmStateAction
{
	[RequiredField]
	public FsmString achievementId;

	[RequiredField]
	public FsmFloat achievementProgress;

	public override void OnEnter()
	{
		Core.AchievementsManager.AddAchievementProgress(achievementId.Value, achievementProgress.Value);
		Finish();
	}
}
