using System.Collections;
using UnityEngine;

public class TutorialLevelDoor : AbstractLevelInteractiveEntity
{
	private bool activated;

	[SerializeField]
	private bool isChaliceTutorial;

	protected override void Activate()
	{
		if (!activated)
		{
			base.Activate();
			StartCoroutine(go_cr());
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CupheadTime.SetLayerSpeed(CupheadTime.Layer.Player, 1f);
	}

	private IEnumerator go_cr()
	{
		activated = true;
		LevelCoin.OnLevelComplete();
		if (isChaliceTutorial)
		{
			PlayerData.Data.IsChaliceTutorialCompleted = true;
		}
		else
		{
			PlayerData.Data.IsTutorialCompleted = true;
		}
		PlayerData.SaveCurrentFile();
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				allPlayer.DisableInput();
				allPlayer.PauseAll();
			}
		}
		TutorialLevel level = Level.Current as TutorialLevel;
		if ((bool)level)
		{
			level.GoBackToHouse();
		}
		else
		{
			ChaliceTutorialLevel chaliceTutorialLevel = Level.Current as ChaliceTutorialLevel;
			if ((bool)chaliceTutorialLevel)
			{
				chaliceTutorialLevel.Exit();
			}
		}
		yield return CupheadTime.WaitForSeconds(this, 0.2f);
		if (isChaliceTutorial)
		{
			SceneLoader.LoadScene(Scenes.scene_map_world_DLC, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
		}
		else
		{
			SceneLoader.LoadScene(Scenes.scene_level_house_elder_kettle, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris);
		}
	}
}
