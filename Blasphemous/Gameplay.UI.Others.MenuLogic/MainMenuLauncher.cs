using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.UI.Others.MenuLogic;

public class MainMenuLauncher : MonoBehaviour
{
	[BoxGroup("InitialScene", true, false, 0)]
	[SerializeField]
	private bool OverwriteInitialScene;

	[BoxGroup("InitialScene", true, false, 0)]
	[ShowIf("OverwriteInitialScene", true)]
	private string initialSceneName = string.Empty;

	private void Start()
	{
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		penitent.gameObject.SetActive(value: false);
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		string newInitialScene = ((!OverwriteInitialScene) ? string.Empty : initialSceneName);
		UIController.instance.ShowMainMenu(newInitialScene);
	}
}
