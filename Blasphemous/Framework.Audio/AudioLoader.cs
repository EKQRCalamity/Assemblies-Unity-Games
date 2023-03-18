using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Tools.Level.Layout;
using UnityEngine;

namespace Framework.Audio;

public class AudioLoader : MonoBehaviour
{
	public FMODAudioCatalog[] AudioCatalogs;

	private void Awake()
	{
		AddCatalogItems();
	}

	private void Start()
	{
		RegisterCatalogs();
	}

	private void RegisterCatalogs()
	{
		if (AudioCatalogs.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < AudioCatalogs.Length; i++)
		{
			if (AudioCatalogs[i] != null)
			{
				Core.Audio.RegisterCatalog(AudioCatalogs[i]);
			}
		}
	}

	private void AddCatalogItems()
	{
		List<FMODAudioCatalog> list = AudioCatalogs.ToList();
		FMODAudioCatalog[] enemiesAudioCatalogs = Core.Audio.EnemiesAudioCatalogs;
		IEnumerable<Type> levelEnemyTypes = GetLevelEnemyTypes();
		foreach (Type item in levelEnemyTypes)
		{
			FMODAudioCatalog[] array = enemiesAudioCatalogs;
			foreach (FMODAudioCatalog fMODAudioCatalog in array)
			{
				if (!(fMODAudioCatalog.Owner == null) && fMODAudioCatalog.Owner.GetType() == item && !list.Contains(fMODAudioCatalog))
				{
					list.Add(fMODAudioCatalog);
					if (fMODAudioCatalog.HasAssociatedCatalog && !list.Contains(fMODAudioCatalog.AssociatedCatalog))
					{
						list.Add(fMODAudioCatalog.AssociatedCatalog);
					}
					break;
				}
			}
		}
		AudioCatalogs = list.ToArray();
	}

	private IEnumerable<Type> GetLevelEnemyTypes()
	{
		List<Type> list = new List<Type>();
		EnemySpawnPoint[] array = UnityEngine.Object.FindObjectsOfType<EnemySpawnPoint>();
		EnemySpawnPoint[] array2 = array;
		foreach (EnemySpawnPoint enemySpawnPoint in array2)
		{
			if (!(enemySpawnPoint.SelectedEnemy == null))
			{
				Type type = enemySpawnPoint.SelectedEnemy.GetComponentInChildren<Enemy>().GetType();
				if (!list.Contains(type))
				{
					list.Add(type);
				}
			}
		}
		return list;
	}
}
