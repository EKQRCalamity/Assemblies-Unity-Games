using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeLevelBackground : LevelProperties.Bee.Entity
{
	public const float GROUP_OFFSET = 455f;

	[SerializeField]
	private BeeLevelPlatforms platformGroup;

	[SerializeField]
	private BeeLevelBackgroundGroup[] groups;

	[SerializeField]
	private Transform[] middleGroups;

	[SerializeField]
	private ScrollingSprite back;

	private BeeLevel level;

	private void Start()
	{
		level = Level.Current as BeeLevel;
		StartCoroutine(middle_cr());
	}

	private void Update()
	{
		back.speed = (0f - level.Speed) * 0.35f;
	}

	public override void LevelInit(LevelProperties.Bee properties)
	{
		base.LevelInit(properties);
		int[] array = new int[groups.Length];
		List<int> list = new List<int>();
		for (int i = 0; i < groups.Length; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < groups.Length; j++)
		{
			int index = Random.Range(0, list.Count);
			array[j] = list[index];
			list.RemoveAt(index);
			groups[array[j]].Init(platformGroup, groups.Length);
			groups[array[j]].SetY(-455f * (float)j);
		}
		platformGroup.gameObject.SetActive(value: false);
	}

	private IEnumerator middle_cr()
	{
		SpriteRenderer[] sprites = new SpriteRenderer[middleGroups.Length];
		for (int j = 0; j < middleGroups.Length; j++)
		{
			sprites[j] = middleGroups[j].GetComponentInChildren<SpriteRenderer>();
			middleGroups[j].gameObject.SetActive(value: false);
		}
		int scale = ((Random.value > 0.5f) ? 1 : (-1));
		while (true)
		{
			int i = Random.Range(0, middleGroups.Length);
			float height = (int)sprites[i].sprite.bounds.size.y;
			float y = (720f + height) / 2f;
			middleGroups[i].gameObject.SetActive(value: true);
			middleGroups[i].SetPosition(0f, y, 0f);
			middleGroups[i].SetScale(scale, 1f, 1f);
			while (middleGroups[i].position.y >= 0f - y)
			{
				middleGroups[i].AddPosition(0f, level.Speed * 0.75f * (float)CupheadTime.Delta);
				yield return null;
			}
			middleGroups[i].gameObject.SetActive(value: false);
			yield return null;
		}
	}
}
