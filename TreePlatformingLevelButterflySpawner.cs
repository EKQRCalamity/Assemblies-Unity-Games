using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlatformingLevelButterflySpawner : AbstractPausableComponent
{
	private const int BUTTERFLIES = 5;

	private const float OFFSET = 50f;

	[SerializeField]
	private TreePlatformingLevelButterfly butterflySmall;

	[SerializeField]
	private Transform startButterflies;

	[SerializeField]
	private Transform endButterflies;

	private List<TreePlatformingLevelButterfly> butterflies;

	private MinMax delay = new MinMax(1.5f, 3f);

	private MinMax velocityX = new MinMax(300f, 600f);

	private MinMax velocityY = new MinMax(100f, 200f);

	private float initalDelay = 6f;

	protected override void Awake()
	{
		base.Awake();
		StartCoroutine(instantiate_butterflies());
		StartCoroutine(spawner_cr());
	}

	private IEnumerator instantiate_butterflies()
	{
		butterflies = new List<TreePlatformingLevelButterfly>();
		yield return CupheadTime.WaitForSeconds(this, 0.1f);
		for (int i = 0; i < 5; i++)
		{
			TreePlatformingLevelButterfly treePlatformingLevelButterfly = Object.Instantiate(butterflySmall);
			treePlatformingLevelButterfly.transform.parent = base.transform;
			treePlatformingLevelButterfly.transform.position = new Vector3(base.transform.position.x, base.transform.position.y - 10000f);
			butterflies.Add(treePlatformingLevelButterfly);
		}
		yield return null;
	}

	private IEnumerator spawner_cr()
	{
		bool keepChecking = true;
		TreePlatformingLevelButterfly spawn = null;
		yield return CupheadTime.WaitForSeconds(this, initalDelay);
		while (true)
		{
			if (PlayerManager.GetNext().transform.position.x < startButterflies.position.x)
			{
				yield return null;
				continue;
			}
			if (endButterflies != null)
			{
				while (PlayerManager.GetNext().transform.position.y > endButterflies.position.y)
				{
					yield return null;
				}
			}
			keepChecking = true;
			while (keepChecking)
			{
				foreach (TreePlatformingLevelButterfly butterfly in butterflies)
				{
					if (!butterfly.isActive)
					{
						spawn = butterfly;
						keepChecking = false;
						break;
					}
				}
				yield return null;
			}
			bool onLeft = Rand.Bool();
			float y = Random.Range(CupheadLevelCamera.Current.Bounds.yMin + 50f, CupheadLevelCamera.Current.Bounds.yMax - 50f);
			float x = ((!onLeft) ? (CupheadLevelCamera.Current.Bounds.xMax + 50f) : (CupheadLevelCamera.Current.Bounds.xMin - 50f));
			float scale = (onLeft ? 1 : (-1));
			spawn.transform.position = new Vector3(x, y);
			spawn.Init(new Vector2((!onLeft) ? (0f - velocityX.RandomFloat()) : velocityX.RandomFloat(), (!Rand.Bool()) ? (-(int)velocityY) : ((int)velocityY)), scale, Random.Range(1, 5), velocityX);
			yield return CupheadTime.WaitForSeconds(this, delay.RandomFloat());
			yield return null;
		}
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawLine(startButterflies.position + new Vector3(0f, 1000f), startButterflies.position + new Vector3(0f, -1000f));
		Gizmos.color = Color.yellow;
		if (endButterflies != null)
		{
			Gizmos.DrawLine(endButterflies.position + new Vector3(1000f, 0f), endButterflies.position + new Vector3(-1000f, 0f));
		}
	}
}
