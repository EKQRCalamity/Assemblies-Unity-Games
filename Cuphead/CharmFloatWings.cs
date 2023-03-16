using UnityEngine;

public class CharmFloatWings : MonoBehaviour
{
	[SerializeField]
	private Effect feather;

	[SerializeField]
	private Effect featherAlt;

	[SerializeField]
	private bool useWindEffect;

	private float spawnFreq = 0.1f;

	private float spawnTime;

	private void Start()
	{
		if (useWindEffect)
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			SpriteRenderer[] array = componentsInChildren;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if (!useWindEffect)
		{
			SpawnFeathers();
		}
	}

	private void OnDisable()
	{
		if (!useWindEffect)
		{
			SpawnFeathers();
		}
	}

	private void SpawnFeathers()
	{
		for (int i = 0; i < 10; i++)
		{
			feather.Create(base.transform.position, new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f)));
		}
	}

	private void Update()
	{
		if (useWindEffect)
		{
			spawnTime += CupheadTime.Delta;
			if (spawnTime > spawnFreq)
			{
				spawnTime -= spawnFreq;
				featherAlt.Create(base.transform.parent.position + Vector3.down * 10f);
			}
		}
	}
}
