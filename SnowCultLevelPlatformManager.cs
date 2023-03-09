using System.Collections.Generic;
using UnityEngine;

public class SnowCultLevelPlatformManager : AbstractCollidableObject
{
	private const int NUM_OF_PLATFORMS = 20;

	[SerializeField]
	private SnowCultLevelPlatform platformPrefab;

	private List<SnowCultLevelPlatform> platforms;

	private void Start()
	{
		platforms = new List<SnowCultLevelPlatform>();
		InstantiatePlatforms();
	}

	private void InstantiatePlatforms()
	{
		for (int i = 0; i < 20; i++)
		{
			SnowCultLevelPlatform snowCultLevelPlatform = Object.Instantiate(platformPrefab);
			snowCultLevelPlatform.gameObject.SetActive(value: false);
			snowCultLevelPlatform.transform.parent = base.transform;
			platforms.Add(snowCultLevelPlatform);
		}
	}
}
