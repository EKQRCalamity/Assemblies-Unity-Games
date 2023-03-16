using System.Collections;
using UnityEngine;

public class PlatformingLevelEnd : AbstractMonoBehaviour
{
	private const string NAME = "PLATFORMING_LEVEL_END_CONTROLER";

	private const float WIN_FADE_TIME = 3f;

	private bool winReadyToExit;

	protected override void Awake()
	{
		base.Awake();
		base.transform.SetAsFirstSibling();
		base.gameObject.name = "PLATFORMING_LEVEL_END_CONTROLER";
	}

	private static PlatformingLevelEnd Create()
	{
		GameObject gameObject = new GameObject();
		return gameObject.AddComponent<PlatformingLevelEnd>();
	}

	public static void Win()
	{
		PlatformingLevelEnd platformingLevelEnd = Create();
		platformingLevelEnd.StartCoroutine(platformingLevelEnd.win_cr());
	}

	private void OnWinComplete()
	{
		winReadyToExit = true;
	}

	private IEnumerator win_cr()
	{
		PlatformingLevelExit.OnWinCompleteEvent += OnWinComplete;
		PauseManager.Pause();
		PlatformingLevelWinAnimation bravoAnimation = PlatformingLevelWinAnimation.Create();
		AudioManager.Play("platforming_announcer_bravo");
		while (bravoAnimation.CurrentState == PlatformingLevelWinAnimation.State.Paused)
		{
			yield return null;
		}
		PauseManager.Unpause();
		CupheadTime.SetAll(1f);
		foreach (LevelPlayerController allPlayer in PlayerManager.GetAllPlayers())
		{
			if (!(allPlayer == null))
			{
				allPlayer.OnLevelWin();
			}
		}
		AbstractProjectile[] array = Object.FindObjectsOfType<AbstractProjectile>();
		foreach (AbstractProjectile abstractProjectile in array)
		{
			abstractProjectile.OnLevelEnd();
		}
		AbstractPlatformingLevelEnemy[] array2 = Object.FindObjectsOfType<AbstractPlatformingLevelEnemy>();
		foreach (AbstractPlatformingLevelEnemy abstractPlatformingLevelEnemy in array2)
		{
			abstractPlatformingLevelEnemy.OnLevelEnd();
		}
		yield return null;
		while (!winReadyToExit)
		{
			yield return null;
		}
		SceneLoader.properties.transitionStart = SceneLoader.Transition.Fade;
		SceneLoader.properties.transitionStartTime = 3f;
		SceneLoader.LoadScene(Scenes.scene_win, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
	}

	public static void Lose()
	{
		PlatformingLevelEnd platformingLevelEnd = Create();
		platformingLevelEnd.StartCoroutine(platformingLevelEnd.lose_cr());
	}

	private IEnumerator lose_cr()
	{
		PauseManager.Unpause();
		AbstractPausableComponent[] array = Object.FindObjectsOfType<AbstractPausableComponent>();
		foreach (AbstractPausableComponent abstractPausableComponent in array)
		{
			abstractPausableComponent.OnLevelEnd();
		}
		LevelGameOverGUI.Current.In(secretTriggered: false);
		yield return null;
	}
}
