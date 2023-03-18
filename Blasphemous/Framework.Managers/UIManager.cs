using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using Gameplay.UI.Widgets;
using UnityEngine;

namespace Framework.Managers;

public class UIManager : GameSystem
{
	private const string ENEMY_HEATLH_BAR_PREFAB = "Core/EnemyHealthBar";

	private const int ENEMY_HEATLH_BAR_POOL_COUNT = 30;

	private GameObject healthBarPrefab;

	public bool ShowGamePlayUI { get; set; }

	public bool ShowGamePlayUIForDebug { get; set; }

	public bool ConsoleShowDebugUI { get; set; }

	public FadeWidget Fade { get; private set; }

	public GlowWidget Glow { get; private set; }

	public CinematicBars Cinematic { get; private set; }

	public GameplayWidget GameplayUI { get; private set; }

	public NavigationWidget NavigationUI { get; private set; }

	public bool MustShowGamePlayUI()
	{
		return ShowGamePlayUI && ShowGamePlayUIForDebug;
	}

	public override void Initialize()
	{
		ShowGamePlayUI = true;
		ShowGamePlayUIForDebug = true;
		ConsoleShowDebugUI = true;
		LevelManager.OnGenericsElementsLoaded += RefreshReferences;
		healthBarPrefab = (GameObject)Resources.Load("Core/EnemyHealthBar");
	}

	public void AttachHealthBarToEnemy(Enemy enemy)
	{
		GameObject gameObject = Object.Instantiate(healthBarPrefab, enemy.transform);
		gameObject.GetComponent<EnemyHealthBar>().UpdateParent(enemy);
	}

	private void RefreshReferences()
	{
		Log.Trace("UI", "Refreshing UI widget references.");
		if (Glow == null)
		{
			Glow = Object.FindObjectOfType<GlowWidget>();
		}
		if (Fade == null)
		{
			Fade = Object.FindObjectOfType<FadeWidget>();
		}
		if (GameplayUI == null)
		{
			GameplayUI = Object.FindObjectOfType<GameplayWidget>();
		}
		if (Cinematic == null)
		{
			Cinematic = Object.FindObjectOfType<CinematicBars>();
		}
		if (NavigationUI == null)
		{
			NavigationUI = Object.FindObjectOfType<NavigationWidget>();
		}
	}
}
