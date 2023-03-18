using Framework.FrameworkCore;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Disables TPO Stunt Fall.")]
public class DisableStuntFall : FsmStateAction
{
	private float oldMaxVSpeedFallStunt;

	public override void OnEnter()
	{
		oldMaxVSpeedFallStunt = Core.Logic.Penitent.AnimatorInyector.MaxVSpeedFallStunt;
		Core.Logic.Penitent.AnimatorInyector.MaxVSpeedFallStunt = float.PositiveInfinity;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		Finish();
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		if (newLevel != oldLevel)
		{
			Core.Logic.Penitent.AnimatorInyector.MaxVSpeedFallStunt = oldMaxVSpeedFallStunt;
		}
	}
}
