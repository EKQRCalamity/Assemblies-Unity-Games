using UnityEngine;

public class TreePlatformingLevelDragonflyProjectileHandler : PlatformingLevelEnemySpawner
{
	[SerializeField]
	private string delaySpawnString;

	private TreePlatformingLevelDragonflyShot[] dragonflyShots;

	private int spawnIndex;

	protected override void Start()
	{
		base.Start();
		dragonflyShots = new TreePlatformingLevelDragonflyShot[GetComponentsInChildren<TreePlatformingLevelDragonflyShot>().Length];
		dragonflyShots = GetComponentsInChildren<TreePlatformingLevelDragonflyShot>();
		spawnIndex = Random.Range(0, delaySpawnString.Split(',').Length);
	}

	protected override void Spawn()
	{
		spawnDelay.min = Parser.FloatParse(delaySpawnString.Split(',')[spawnIndex]);
		spawnDelay.max = Parser.FloatParse(delaySpawnString.Split(',')[spawnIndex]);
		Activate();
		base.Spawn();
		spawnIndex = (spawnIndex + 1) % delaySpawnString.Split(',').Length;
	}

	private void Activate()
	{
		int num = Random.Range(0, dragonflyShots.Length);
		if (!dragonflyShots[num].isActivated)
		{
			dragonflyShots[num].Activate();
		}
	}
}
