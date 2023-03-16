using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroArcadeBouncyManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private RetroArcadeBouncyBallHolder ballHolder;

	[SerializeField]
	private Transform[] spawnPoints;

	private const int BALLCOUNT = 3;

	public void StartBouncy()
	{
		StartCoroutine(spawn_balls_cr());
	}

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.color = Color.black;
		Transform[] array = spawnPoints;
		foreach (Transform transform in array)
		{
			Gizmos.DrawWireSphere(transform.position, 50f);
		}
	}

	private IEnumerator spawn_balls_cr()
	{
		LevelProperties.RetroArcade.Bouncy p = base.properties.CurrentState.bouncy;
		int counter = 0;
		List<RetroArcadeBouncyBallHolder> holders = new List<RetroArcadeBouncyBallHolder>();
		int typeMainIndex = Random.Range(0, p.typeString.Length);
		string[] typeString2 = p.typeString[typeMainIndex].Split(',');
		int typeIndex = Random.Range(0, typeString2.Length);
		string[] ballTypes = new string[3];
		while (counter < p.waveCount)
		{
			typeString2 = p.typeString[typeMainIndex].Split(',');
			int posIndex = Random.Range(0, spawnPoints.Length);
			for (int i = 0; i < 3; i++)
			{
				ballTypes[i] = typeString2[typeIndex];
				if (typeIndex < typeString2.Length - 1)
				{
					typeIndex++;
					continue;
				}
				typeMainIndex = (typeMainIndex + 1) % p.typeString.Length;
				typeIndex = 0;
			}
			RetroArcadeBouncyBallHolder holder = ballHolder.Create(this, p, spawnPoints[posIndex].position, ballTypes);
			holders.Add(holder);
			counter++;
			yield return CupheadTime.WaitForSeconds(this, p.spawnRange.RandomFloat());
		}
		bool allDead2 = true;
		while (true)
		{
			allDead2 = true;
			for (int j = 0; j < holders.Count; j++)
			{
				if (!holders[j].IsDead)
				{
					allDead2 = false;
				}
			}
			if (allDead2)
			{
				break;
			}
			yield return null;
		}
		base.properties.DealDamageToNextNamedState();
		foreach (RetroArcadeBouncyBallHolder item in holders)
		{
			item.DestroyBallsHeld();
			Object.Destroy(item.gameObject);
		}
		yield return null;
	}
}
