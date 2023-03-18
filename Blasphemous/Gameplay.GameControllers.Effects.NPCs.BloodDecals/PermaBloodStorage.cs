using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.NPCs.BloodDecals;

public class PermaBloodStorage
{
	public struct PermaBloodMemento
	{
		public PermaBlood.PermaBloodType type;

		public Vector2 position;

		public Quaternion rotation;

		public PermaBloodMemento(PermaBlood.PermaBloodType _type, Vector2 _position, Quaternion _rotation)
		{
			type = _type;
			position = _position;
			rotation = _rotation;
		}
	}

	protected Dictionary<int, List<PermaBloodMemento>> generalStore;

	public PermaBloodStorage()
	{
		generalStore = new Dictionary<int, List<PermaBloodMemento>>();
	}

	public List<PermaBloodMemento> GetPermaBloodSceneList(int sceneIndex)
	{
		if (generalStore.TryGetValue(sceneIndex, out var value))
		{
			return value;
		}
		return null;
	}

	public void AddPermaBloodToSceneList(int sceneIndex, PermaBloodMemento memento)
	{
		List<PermaBloodMemento> permaBloodSceneList = GetPermaBloodSceneList(sceneIndex);
		if (permaBloodSceneList != null)
		{
			permaBloodSceneList.Add(memento);
			return;
		}
		permaBloodSceneList = new List<PermaBloodMemento>();
		permaBloodSceneList.Add(memento);
		generalStore.Add(sceneIndex, permaBloodSceneList);
	}

	public void ClearSceneStore(int sceneIndex)
	{
		GetPermaBloodSceneList(sceneIndex)?.Clear();
	}

	public void RemoveGeneralStore()
	{
		if (generalStore.Count > 0)
		{
			generalStore.Clear();
		}
	}
}
