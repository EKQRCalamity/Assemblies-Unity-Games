using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Grants an Achievement.")]
public class GrantAchievement : FsmStateAction
{
	[RequiredField]
	public FsmString achievementId;

	public override void OnEnter()
	{
		Core.AchievementsManager.GrantAchievement(achievementId.Value);
		Finish();
	}
}
