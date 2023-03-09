using System;
using System.Collections;
using UnityEngine;

public class LevelEnd : AbstractMonoBehaviour
{
	private const string NAME = "LEVEL_END_CONTROLER";

	private const float WIN_FADE_TIME = 3f;

	private const float JOIN_WAIT = 1f;

	protected override void Awake()
	{
		base.Awake();
		base.transform.SetAsFirstSibling();
		base.gameObject.name = "LEVEL_END_CONTROLER";
	}

	private static LevelEnd Create()
	{
		GameObject gameObject = new GameObject();
		return gameObject.AddComponent<LevelEnd>();
	}

	public static void Win(IEnumerator knockoutSFXCoroutine, Action onBossDeathCallback, Action explosionsCallback, Action explosionsFalloffCallback, Action explosionsEndCallback, AbstractPlayerController[] players, float bossDeathTime, bool goToWinScreen, bool isMausoleum, bool isDevil, bool isTowerOfPower)
	{
		LevelEnd levelEnd = Create();
		levelEnd.StartCoroutine(levelEnd.win_cr(knockoutSFXCoroutine, onBossDeathCallback, explosionsCallback, explosionsFalloffCallback, explosionsEndCallback, players, bossDeathTime, goToWinScreen, isMausoleum, isDevil, isTowerOfPower));
	}

	private IEnumerator win_cr(IEnumerator knockoutSFXCoroutine, Action onBossDeathCallback, Action explosionsCallback, Action explosionsFalloffCallback, Action explosionsEndCallback, AbstractPlayerController[] players, float bossDeathTime, bool goToWinScreen, bool isMausoleum, bool isDevil, bool isTowerOfPower)
	{
		PauseManager.Pause();
		LevelKOAnimation koAnim = LevelKOAnimation.Create(isMausoleum);
		if (Level.IsChessBoss)
		{
			AudioManager.StartBGMAlternate(0);
		}
		if (Level.Current.CurrentLevel == Levels.Saltbaker)
		{
			AudioManager.StartBGMAlternate(2);
		}
		StartCoroutine(knockoutSFXCoroutine);
		yield return koAnim.StartCoroutine(koAnim.anim_cr());
		PauseManager.Unpause();
		explosionsCallback();
		CupheadTime.SetAll(1f);
		if (!isMausoleum)
		{
			foreach (AbstractPlayerController allPlayer in PlayerManager.GetAllPlayers())
			{
				if (!(allPlayer == null))
				{
					allPlayer.OnLevelWin();
				}
			}
		}
		onBossDeathCallback?.Invoke();
		yield return new WaitForSeconds(bossDeathTime + 0.3f);
		AbstractProjectile[] array = UnityEngine.Object.FindObjectsOfType<AbstractProjectile>();
		foreach (AbstractProjectile abstractProjectile in array)
		{
			abstractProjectile.OnLevelEnd();
		}
		if (Level.IsTowerOfPower)
		{
			TowerOfPowerLevelGameInfo.SetPlayersStats(PlayerId.PlayerOne);
			if (PlayerManager.Multiplayer)
			{
				TowerOfPowerLevelGameInfo.SetPlayersStats(PlayerId.PlayerTwo);
			}
		}
		else if (Level.IsDicePalace && !Level.IsDicePalaceMain)
		{
			DicePalaceMainLevelGameInfo.SetPlayersStats();
		}
		SceneLoader.properties.transitionStart = SceneLoader.Transition.Fade;
		SceneLoader.properties.transitionStartTime = 3f;
		if (Level.IsChessBoss || Level.Current.CurrentLevel == Levels.Saltbaker)
		{
			yield return new WaitForSeconds(2f);
		}
		if (goToWinScreen)
		{
			SceneLoader.LoadScene(Scenes.scene_win, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
		}
		else if (Level.IsTowerOfPower)
		{
			SceneLoader.ContinueTowerOfPower();
		}
		else if (Level.IsGraveyard)
		{
			SceneLoader.LoadScene(Scenes.scene_map_world_DLC, SceneLoader.Transition.Fade, SceneLoader.Transition.Iris, SceneLoader.Icon.None);
		}
		else if (Level.IsChessBoss)
		{
			if (SceneLoader.CurrentContext is GauntletContext)
			{
				int currentIndex = Array.IndexOf(Level.kingOfGamesLevels, Level.Current.CurrentLevel);
				int num = MathUtilities.NextIndex(currentIndex, Level.kingOfGamesLevels.Length);
				if (num == 0)
				{
					SceneLoader.LoadScene(Scenes.scene_level_chess_castle, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None, new GauntletContext(complete: true));
				}
				else
				{
					Levels level = Level.kingOfGamesLevels[num];
					SceneLoader.Transition transitionStart = SceneLoader.Transition.Fade;
					GauntletContext context = new GauntletContext(complete: false);
					SceneLoader.LoadLevel(level, transitionStart, SceneLoader.Icon.Hourglass, context);
				}
			}
			else
			{
				SceneLoader.LoadScene(Scenes.scene_level_chess_castle, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
			}
		}
		else if (!isMausoleum)
		{
			SceneLoader.ReloadLevel();
		}
		yield return new WaitForSeconds(2.5f);
		explosionsEndCallback();
	}

	public static void Lose(bool isMausoleum, bool secretTriggered)
	{
		LevelEnd levelEnd = Create();
		levelEnd.StartCoroutine(levelEnd.lose_cr(isMausoleum, secretTriggered));
	}

	private IEnumerator lose_cr(bool isMausoleum, bool secretTriggered)
	{
		if (isMausoleum)
		{
			AudioManager.Play("level_announcer_fail");
		}
		PauseManager.Unpause();
		AbstractPausableComponent[] array = UnityEngine.Object.FindObjectsOfType<AbstractPausableComponent>();
		foreach (AbstractPausableComponent abstractPausableComponent in array)
		{
			abstractPausableComponent.OnLevelEnd();
		}
		LevelGameOverGUI.Current.In(secretTriggered);
		if (Level.IsChessBoss)
		{
			yield return new WaitForSeconds(1f);
			AudioManager.StartBGMAlternate(1);
		}
		yield return null;
	}

	public static void PlayerJoined()
	{
		LevelEnd levelEnd = Create();
		levelEnd.StartCoroutine(levelEnd.playerJoined_cr());
	}

	private IEnumerator playerJoined_cr()
	{
		PauseManager.Unpause();
		AbstractPausableComponent[] array = UnityEngine.Object.FindObjectsOfType<AbstractPausableComponent>();
		foreach (AbstractPausableComponent abstractPausableComponent in array)
		{
			abstractPausableComponent.OnLevelEnd();
		}
		yield return new WaitForSeconds(1f);
		yield return new WaitForSeconds(1f);
		SceneLoader.LoadLastMap();
	}
}
