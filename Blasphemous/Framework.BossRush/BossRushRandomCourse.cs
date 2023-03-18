using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.BossRush;

[Serializable]
public class BossRushRandomCourse : BossRushCourse
{
	[InfoBox("Put here all the scenes of the course, fixed in place or not.", InfoMessageType.Info, null)]
	public List<string> ScenesPool = new List<string>();

	[InfoBox("The indexes of these scenes should consider all the scenes in total, and these indexes refer to the scenes pool.", InfoMessageType.Info, null)]
	public List<int> FixedScenesIndexes = new List<int>();

	[InfoBox("The indexes of these scenes should consider all the scenes in total. These indexes will refer to the randomized result.", InfoMessageType.Info, null)]
	public List<int> FontRechargingScenesIndexes = new List<int>();

	[HideInInspector]
	public bool RandomizeNextScenesList;

	private List<string> RandomizedScenes = new List<string>();

	public override List<string> GetScenes()
	{
		if (RandomizeNextScenesList)
		{
			RandomizeNextScenesList = false;
			RandomizeScenesList();
		}
		return RandomizedScenes;
	}

	public override List<string> GetFontRechargingScenes()
	{
		List<string> list = new List<string>();
		if (RandomizedScenes.Count > 0)
		{
			foreach (int fontRechargingScenesIndex in FontRechargingScenesIndexes)
			{
				list.Add(RandomizedScenes[fontRechargingScenesIndex]);
			}
			return list;
		}
		return list;
	}

	[Button(ButtonSizes.Small)]
	private void RandomizeScenesList()
	{
		RandomizedScenes.Clear();
		RandomizedScenes.AddRange(ScenesPool);
		RandomizedScenes = Shuffle(RandomizedScenes);
	}

	private List<string> Shuffle(List<string> scenes)
	{
		for (int i = 0; i < scenes.Count; i++)
		{
			if (!FixedScenesIndexes.Contains(i))
			{
				string value = scenes[i];
				int num = UnityEngine.Random.Range(i, scenes.Count);
				if (!FixedScenesIndexes.Contains(num))
				{
					scenes[i] = scenes[num];
					scenes[num] = value;
				}
			}
		}
		return scenes;
	}
}
