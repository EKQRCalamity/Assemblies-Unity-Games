using System;
using System.Collections;
using System.Linq;
using Framework.Managers;
using HutongGames.PlayMaker;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.PlayMaker.Action;

public class EnableStandByEnemies : FsmStateAction
{
	[ArrayEditor(VariableType.GameObject, "", 0, 0, 65536)]
	public FsmArray EnemySpawnPoint;

	public const string AppearFx = "event:/Key Event/HordeAppear";

	public override void OnEnter()
	{
		base.OnEnter();
		StartCoroutine(SetActiveStandByEnemies());
		Finish();
	}

	private IEnumerator SetActiveStandByEnemies()
	{
		object[] spawnPoints = EnemySpawnPoint.Values;
		System.Random rnd = new System.Random();
		object[] shuffleArray = spawnPoints.OrderBy((object x) => rnd.Next()).ToArray();
		for (int i = 0; i < shuffleArray.Length; i++)
		{
			GameObject go = shuffleArray[i] as GameObject;
			if (go == null)
			{
				continue;
			}
			EnemySpawnPoint spawnPoint = go.GetComponentInChildren<EnemySpawnPoint>();
			yield return new WaitForSeconds(GetSpawnPointDelayInstantiation(spawnPoint));
			if ((bool)spawnPoint.SpawnedEnemy)
			{
				if (spawnPoint.SpawnOnArena)
				{
					spawnPoint.SpawnEnemyOnArena();
					Core.Audio.PlaySfx("event:/Key Event/HordeAppear");
				}
				else if (!spawnPoint.SpawnEnabledEnemy)
				{
					spawnPoint.SpawnedEnemy.gameObject.SetActive(value: true);
				}
			}
		}
	}

	private float GetSpawnPointDelayInstantiation(EnemySpawnPoint spawnPoint)
	{
		float result = 0f;
		if (spawnPoint.SpawnEnabledEnemy || spawnPoint.SpawnOnArena)
		{
			result = UnityEngine.Random.Range(0.1f, 0.5f);
		}
		return result;
	}
}
