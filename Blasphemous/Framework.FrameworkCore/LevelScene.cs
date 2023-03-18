using System;
using System.Collections;
using Framework.Managers;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.FrameworkCore;

public class LevelScene
{
	private AsyncOperation operation;

	private bool fistTimeLoaded = true;

	public string SceneName { get; private set; }

	public string Section { get; private set; }

	public Scene Scene { get; private set; }

	public LevelManager.LevelStatus CurrentStatus { get; private set; }

	public bool InOperation { get; private set; }

	public LevelScene(string name, string section)
	{
		SceneName = name;
		Section = section;
		InOperation = false;
		CurrentStatus = LevelManager.LevelStatus.Unloaded;
	}

	public IEnumerator Load()
	{
		if (CurrentStatus != 0)
		{
			Debug.LogError("Scene LOAD, name: " + SceneName + " -> Try to load with status " + CurrentStatus);
			yield break;
		}
		if (CheckSceneLoadedInEditor())
		{
			Scene = SceneManager.GetSceneByName(SceneName);
			fistTimeLoaded = false;
			CurrentStatus = LevelManager.LevelStatus.Loaded;
			yield break;
		}
		InOperation = true;
		fistTimeLoaded = false;
		operation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
		while (!operation.isDone)
		{
			yield return null;
		}
		CurrentStatus = LevelManager.LevelStatus.Loaded;
		Scene = SceneManager.GetSceneByName(SceneName);
		InOperation = false;
	}

	public IEnumerator Unload()
	{
		if (CurrentStatus == LevelManager.LevelStatus.Unloaded)
		{
			Debug.LogError("Scene UNLOAD, name: " + SceneName + " -> Try to unload with status " + CurrentStatus);
			yield break;
		}
		InOperation = true;
		operation = SceneManager.UnloadSceneAsync(Scene.name);
		yield return operation;
		CurrentStatus = LevelManager.LevelStatus.Unloaded;
		InOperation = false;
	}

	public IEnumerator Activate()
	{
		if (CurrentStatus != LevelManager.LevelStatus.Loaded && CurrentStatus != LevelManager.LevelStatus.Deactivated)
		{
			Debug.LogError("Scene ACTIVATE, name: " + SceneName + " -> Try to activate with status " + CurrentStatus);
			yield break;
		}
		InOperation = true;
		if (fistTimeLoaded)
		{
			fistTimeLoaded = false;
			operation.allowSceneActivation = true;
			yield return new WaitForEndOfFrame();
			yield return null;
			yield return operation;
			Scene = SceneManager.GetSceneByName(SceneName);
		}
		else
		{
			try
			{
				Scene.GetRootGameObjects().ForEach(delegate(GameObject obj)
				{
					obj.SetActive(value: true);
				});
			}
			catch (Exception ex)
			{
				Debug.LogError("Error Activating level");
				Debug.LogError(ex.Message);
			}
		}
		CurrentStatus = LevelManager.LevelStatus.Activated;
		InOperation = false;
	}

	public void DeActivate()
	{
		InOperation = true;
		if (CurrentStatus != LevelManager.LevelStatus.Activated && CurrentStatus != LevelManager.LevelStatus.Loaded)
		{
			Debug.LogError("Scene DEACTIVATE, name: " + SceneName + " -> Try to deactivate with status " + CurrentStatus);
			return;
		}
		try
		{
			Scene.GetRootGameObjects().ForEach(delegate(GameObject obj)
			{
				obj.SetActive(value: false);
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("Error Deactivating level");
			Debug.LogError(ex.Message);
		}
		CurrentStatus = LevelManager.LevelStatus.Deactivated;
		InOperation = false;
	}

	private bool CheckSceneLoadedInEditor()
	{
		bool result = false;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.name == SceneName)
			{
				Scene = sceneAt;
				result = true;
				break;
			}
		}
		return result;
	}
}
