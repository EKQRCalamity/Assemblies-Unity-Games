using System.Collections.Generic;
using Framework.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.GameControllers.Effects.NPCs.BloodDecals;

public class PermaBloodSpawner : MonoBehaviour
{
	public PermaBlood[] permaBloods;

	protected Dictionary<PermaBlood.PermaBloodType, GameObject> permaBloodDic;

	private int currentSceneIndex;

	private void Awake()
	{
		permaBloodDic = getPermaBloodDictionary(permaBloods);
	}

	private void Start()
	{
		spawnPermaBloodInCurrentScene();
	}

	public void spawnPermaBloodInCurrentScene()
	{
		if (permaBloodDic.Count <= 0)
		{
			return;
		}
		currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		List<PermaBloodStorage.PermaBloodMemento> permaBloodSceneList = Core.Logic.PermaBloodStore.GetPermaBloodSceneList(currentSceneIndex);
		if (permaBloodSceneList != null)
		{
			for (int i = 0; i < permaBloodSceneList.Count; i++)
			{
				GameObject original = permaBloodDic[permaBloodSceneList[i].type];
				Vector2 position = permaBloodSceneList[i].position;
				Quaternion rotation = permaBloodSceneList[i].rotation;
				Object.Instantiate(original, position, rotation);
			}
		}
	}

	protected Dictionary<PermaBlood.PermaBloodType, GameObject> getPermaBloodDictionary(PermaBlood[] permaBloods)
	{
		Dictionary<PermaBlood.PermaBloodType, GameObject> dictionary = new Dictionary<PermaBlood.PermaBloodType, GameObject>();
		if (permaBloods.Length > 0)
		{
			for (byte b = 0; b < permaBloods.Length; b = (byte)(b + 1))
			{
				dictionary.Add(permaBloods[b].permaBloodType, permaBloods[b].gameObject);
			}
		}
		return dictionary;
	}
}
