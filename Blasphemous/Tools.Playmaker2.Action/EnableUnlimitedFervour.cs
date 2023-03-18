using System.Collections;
using Framework.FrameworkCore;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Disable Penitent Blood Penance.")]
public class EnableUnlimitedFervour : FsmStateAction
{
	private bool keepRefilling = true;

	public override void OnEnter()
	{
		keepRefilling = true;
		StartCoroutine(RefillFervour());
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		Finish();
	}

	private IEnumerator RefillFervour()
	{
		while (keepRefilling)
		{
			Core.Logic.Penitent.Stats.Fervour.SetToCurrentMax();
			yield return null;
		}
	}

	private void OnLevelLoaded(Framework.FrameworkCore.Level oldLevel, Framework.FrameworkCore.Level newLevel)
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		keepRefilling = false;
	}
}
