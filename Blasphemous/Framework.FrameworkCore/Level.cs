using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using UnityEngine;

namespace Framework.FrameworkCore;

public class Level
{
	public const string LOGIC_SECTION = "LOGIC";

	public const string MAIN_SECTION = "MAIN";

	public static string[] sections = new string[5] { "LAYOUT", "DECO", "AUDIO", "MAIN", "LOGIC" };

	private Dictionary<string, LevelScene> scenes;

	public string LevelName { get; private set; }

	public LevelManager.LevelStatus CurrentStatus { get; private set; }

	public bool IsBundle { get; private set; }

	public int Distance { get; set; }

	public Level(string name, bool bundle = true)
	{
		CurrentStatus = LevelManager.LevelStatus.Unloaded;
		IsBundle = bundle;
		LevelName = name;
		scenes = new Dictionary<string, LevelScene>();
		Distance = 1;
	}

	public LevelScene GetLogicScene()
	{
		return scenes["LOGIC"];
	}

	public IEnumerator Load(bool streaming)
	{
		if (CurrentStatus != 0)
		{
			Debug.LogError("Level LOAD, name: " + LevelName + " -> Try to load with status " + CurrentStatus);
			yield break;
		}
		CurrentStatus = LevelManager.LevelStatus.Loading;
		if (IsBundle)
		{
			string[] array = sections;
			foreach (string section in array)
			{
				string sceneName = LevelName + "_" + section;
				LevelScene scene2 = new LevelScene(sceneName, section);
				scenes[section] = scene2;
				if (section != "LOGIC")
				{
					yield return scene2.Load();
					if (streaming)
					{
						scene2.DeActivate();
					}
				}
			}
		}
		else
		{
			LevelScene scene = new LevelScene(LevelName, "LOGIC");
			scenes["LOGIC"] = scene;
			yield return scene.Load();
			if (streaming)
			{
				scene.DeActivate();
			}
		}
		CurrentStatus = LevelManager.LevelStatus.Loaded;
	}

	public IEnumerator UnLoad()
	{
		if (CurrentStatus != LevelManager.LevelStatus.Loaded && CurrentStatus != LevelManager.LevelStatus.Deactivated)
		{
			Debug.LogError("Level UNLOAD, name: " + LevelName + " -> Try to unload with status " + CurrentStatus);
			yield break;
		}
		CurrentStatus = LevelManager.LevelStatus.Unloading;
		foreach (LevelScene scene in scenes.Values)
		{
			if (scene.Section == "LOGIC")
			{
				if (scene.CurrentStatus == LevelManager.LevelStatus.Activated)
				{
					yield return scene.Unload();
				}
			}
			else
			{
				yield return scene.Unload();
			}
		}
		CurrentStatus = LevelManager.LevelStatus.Unloaded;
	}

	public IEnumerator Activate()
	{
		if (CurrentStatus != LevelManager.LevelStatus.Loaded && CurrentStatus != LevelManager.LevelStatus.Deactivated)
		{
			Debug.LogError("Level ACTIVATE, name: " + LevelName + " -> Try to activate with status " + CurrentStatus);
			yield break;
		}
		CurrentStatus = LevelManager.LevelStatus.Activating;
		foreach (LevelScene scene in scenes.Values)
		{
			if (scene.Section == "LOGIC" && scene.CurrentStatus == LevelManager.LevelStatus.Unloaded)
			{
				yield return scene.Load();
			}
			yield return scene.Activate();
		}
		Resources.UnloadUnusedAssets();
		CurrentStatus = LevelManager.LevelStatus.Activated;
	}

	public IEnumerator DeActivate()
	{
		if (CurrentStatus != LevelManager.LevelStatus.Activated)
		{
			Debug.LogError("Level DEACTIVATE, name: " + LevelName + " -> Try to deactivate with status " + CurrentStatus);
			yield break;
		}
		CurrentStatus = LevelManager.LevelStatus.Deactivating;
		foreach (LevelScene scene in scenes.Values)
		{
			if (scene.Section == "LOGIC")
			{
				yield return scene.Unload();
				continue;
			}
			scene.DeActivate();
			yield return 0;
		}
		CurrentStatus = LevelManager.LevelStatus.Deactivated;
	}
}
