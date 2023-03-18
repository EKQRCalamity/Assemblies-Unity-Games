using Framework.FrameworkCore;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Disable Penitent Blood Penance.")]
public class DisablePenitentBloodPenance : FsmStateAction
{
	public override void OnEnter()
	{
		Core.Logic.Penitent.Penance.enabled = false;
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		Finish();
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		if (newLevel != oldLevel)
		{
			Core.Logic.Penitent.Penance.enabled = true;
		}
	}
}
