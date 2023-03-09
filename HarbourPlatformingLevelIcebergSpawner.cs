using UnityEngine;

public class HarbourPlatformingLevelIcebergSpawner : PlatformingLevelEnemySpawner
{
	[SerializeField]
	private HarbourPlatformingLevelIceberg[] icebergPrefabs;

	[SerializeField]
	private string spawnDelayString = "5.5,7.0";

	private string[] spawn;

	private int spawnIndex;

	protected override void Start()
	{
		base.Start();
		spawn = spawnDelayString.Split(',');
		spawnIndex = Random.Range(0, spawn.Length);
	}

	protected override void Spawn()
	{
		spawnDelay.min = Parser.FloatParse(spawn[spawnIndex]);
		spawnDelay.max = Parser.FloatParse(spawn[spawnIndex]);
		int num = Random.Range(0, icebergPrefabs.Length);
		float x = CupheadLevelCamera.Current.transform.position.x + CupheadLevelCamera.Current.Width / 2f + (icebergPrefabs[num].GetComponent<Renderer>().bounds.size.x + icebergPrefabs[num].GetComponent<Renderer>().bounds.size.x / 2f);
		float y = CupheadLevelCamera.Current.transform.position.y - 100f;
		icebergPrefabs[num].Spawn(new Vector3(x, y));
		base.Spawn();
		spawnIndex = (spawnIndex + 1) % spawn.Length;
	}
}
