using System.Collections.Generic;
using Gameplay.GameControllers.Entities;
using HutongGames.PlayMaker;
using Tools.Level.Layout;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.PlayMaker.Action;

[ActionCategory(ActionCategory.Array)]
[HutongGames.PlayMaker.Tooltip("Spawns a group of enemies set in their spawn points")]
public class SpawnEnemies : FsmStateAction
{
	[HutongGames.PlayMaker.Tooltip("The set of enemy spawners.")]
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray EnemySpawners;

	public override void Reset()
	{
		EnemySpawners = null;
	}

	public override void OnEnter()
	{
		CastEnemySpawners();
		Finish();
	}

	private void CastEnemySpawners()
	{
		List<Enemy> list = FindObjectsOfTypeAll<Enemy>();
		object[] values = EnemySpawners.Values;
		foreach (object obj in values)
		{
			GameObject gameObject = obj as GameObject;
			if (!gameObject)
			{
				continue;
			}
			EnemySpawnPoint component = gameObject.GetComponent<EnemySpawnPoint>();
			if (!component || component.EnemySpawnDisabled)
			{
				continue;
			}
			bool flag = false;
			foreach (Enemy item in list)
			{
				if (item.SpawningId != component.gameObject.GetHashCode())
				{
					continue;
				}
				item.gameObject.SetActive(value: true);
				item.Stats.Life.Current = item.Stats.Life.Base;
				item.Status.Dead = false;
				flag = true;
				break;
			}
			if (!flag)
			{
				component.CreateEnemy();
			}
			component.Consumed = false;
		}
	}

	public static List<T> FindObjectsOfTypeAll<T>()
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				foreach (GameObject gameObject in rootGameObjects)
				{
					list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive: true));
				}
			}
		}
		return list;
	}
}
