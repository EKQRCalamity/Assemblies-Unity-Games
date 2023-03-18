using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Gizmos;

public class PenitentSpawnPoint : SpawnPoint
{
	public GameObject PenitentPrefab;

	public GameObject Instance()
	{
		GameObject result = null;
		if (PenitentPrefab != null)
		{
			result = Object.Instantiate(PenitentPrefab, base.transform.position, Quaternion.identity);
		}
		else
		{
			Debug.LogError("The prefab variable cannot be null");
		}
		return result;
	}
}
