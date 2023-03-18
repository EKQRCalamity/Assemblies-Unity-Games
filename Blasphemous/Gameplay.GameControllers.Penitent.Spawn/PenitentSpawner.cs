using Framework.Managers;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Penitent.Spawn;

public class PenitentSpawner
{
	public Core.SimpleEvent OnSpawned;

	public GameObject SpawnPenitent(PenitentSpawnPoint penitentSpawnPoint)
	{
		GameObject gameObject = null;
		if (penitentSpawnPoint != null)
		{
			gameObject = penitentSpawnPoint.Instance();
		}
		if (gameObject != null && OnSpawned != null)
		{
			OnSpawned();
		}
		return gameObject;
	}
}
