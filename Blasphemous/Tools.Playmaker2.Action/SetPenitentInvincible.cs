using System.Collections;
using Framework.Managers;
using HutongGames.PlayMaker;

namespace Tools.Playmaker2.Action;

[ActionCategory("Blasphemous Action")]
[Tooltip("Enables or disables the Invincibility of Penitent.")]
public class SetPenitentInvincible : FsmStateAction
{
	public FsmBool isInvincible;

	private string currentLevelName;

	public override void OnEnter()
	{
		if (isInvincible != null && isInvincible.Value)
		{
			currentLevelName = Core.LevelManager.currentLevel.LevelName;
			StartCoroutine(InvencibilityCoroutine());
		}
		Finish();
	}

	private IEnumerator InvencibilityCoroutine()
	{
		while (currentLevelName.Equals(Core.LevelManager.currentLevel.LevelName))
		{
			yield return null;
			Core.Logic.Penitent.Stats.Life.SetToCurrentMax();
		}
	}
}
