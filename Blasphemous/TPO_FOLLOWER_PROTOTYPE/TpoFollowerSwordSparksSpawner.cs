using Framework.Managers;
using Gameplay.GameControllers.Effects.Player.Sparks;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace TPO_FOLLOWER_PROTOTYPE;

public class TpoFollowerSwordSparksSpawner : MonoBehaviour
{
	[SerializeField]
	private SwordSpark[] swordSparks;

	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Penitent penitent)
	{
		SwordSparkSpawner componentInChildren = penitent.GetComponentInChildren<SwordSparkSpawner>();
		SetDemakeSparks(componentInChildren);
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}

	private void SetDemakeSparks(SwordSparkSpawner sparkSpawner)
	{
		if ((bool)sparkSpawner)
		{
			sparkSpawner.SwordSparks = swordSparks;
		}
	}
}
