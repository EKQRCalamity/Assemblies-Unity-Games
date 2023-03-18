using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Framework.Persistence;

public class PersistentEnemy : PersistentObject
{
	private class PietyPersistenceData : PersistentManager.PersistentData
	{
		public bool IsAlive;

		public Vector3 Position;

		public PietyPersistenceData(string id)
			: base(id)
		{
		}
	}

	public Enemy enemy;

	public List<GameObject> objectsToDisbleWhenDead = new List<GameObject>();

	public List<GameObject> objectsToEnableWhenDead = new List<GameObject>();

	public GameObject SpriteDead;

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		PietyPersistenceData pietyPersistenceData = CreatePersistentData<PietyPersistenceData>();
		pietyPersistenceData.IsAlive = enemy.CurrentLife > 0f;
		pietyPersistenceData.Position = enemy.transform.localPosition;
		return pietyPersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		PietyPersistenceData pietyPersistenceData = (PietyPersistenceData)data;
		enemy.gameObject.SetActive(pietyPersistenceData.IsAlive);
		foreach (GameObject item in objectsToEnableWhenDead)
		{
			if (!(item == null))
			{
				item.SetActive(!pietyPersistenceData.IsAlive);
			}
		}
		foreach (GameObject item2 in objectsToDisbleWhenDead)
		{
			if (!(item2 == null))
			{
				item2.SetActive(pietyPersistenceData.IsAlive);
			}
		}
		if ((bool)SpriteDead)
		{
			SpriteDead.SetActive(!pietyPersistenceData.IsAlive);
			SpriteDead.transform.localPosition = pietyPersistenceData.Position;
		}
	}
}
