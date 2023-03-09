using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroArcadeSheriffManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private Transform bottom;

	[SerializeField]
	private RetroArcadeSheriff sheriffGreenPrefab;

	[SerializeField]
	private RetroArcadeSheriff sheriffYellowPrefab;

	[SerializeField]
	private RetroArcadeSheriff sheriffOrangePrefab;

	private List<RetroArcadeSheriff> sheriffs;

	private List<Vector3> spawnPositions;

	private const float offset = 20f;

	private float delaySubstract;

	public void StartSheriff()
	{
		SetupSpawnPositions();
		StartCoroutine(sheriff_cr());
	}

	private void SetupSpawnPositions()
	{
		int num = 6;
		int num2 = 4;
		float f = bottom.position.y - (float)Level.Current.Ground;
		float num3 = ((float)Level.Current.Right - 20f) / (float)(num - 1) * 2f;
		float num4 = ((float)Level.Current.Ceiling - Mathf.Abs(f) - 20f) / (float)(num2 - 1) * 2f;
		int num5 = 0;
		spawnPositions = new List<Vector3>();
		for (int i = 0; i < num * 2; i++)
		{
			float x = (float)Level.Current.Left + 20f + num3 * (float)num5;
			float y = ((i >= num) ? ((float)Level.Current.Ground + 20f) : ((float)Level.Current.Ceiling - 20f));
			spawnPositions.Add(new Vector3(x, y));
			num5 = ((i != num - 1) ? (++num5) : 0);
		}
		num5 = 1;
		for (int j = 1; j < num2 * 2 - 1; j++)
		{
			float x2 = ((j >= num2) ? ((float)Level.Current.Left + 20f) : ((float)Level.Current.Right - 20f));
			float y2 = (float)Level.Current.Ground + 20f + num4 * (float)num5;
			spawnPositions.Add(new Vector3(x2, y2));
			if (j == num2 - 2)
			{
				num5 = 1;
				j = num2 - 1 + 1;
			}
			else
			{
				num5++;
			}
		}
	}

	private IEnumerator sheriff_cr()
	{
		LevelProperties.RetroArcade.Sheriff p = base.properties.CurrentState.sheriff;
		int delayMainIndex = Random.Range(0, p.delayString.Length);
		string[] delayString = p.delayString[delayMainIndex].Split(',');
		int delayIndex = Random.Range(0, delayString.Length);
		int colorMainIndex = Random.Range(0, p.colorString.Length);
		string[] colorString2 = p.colorString[colorMainIndex].Split(',');
		int colorIndex = Random.Range(0, colorString2.Length);
		RetroArcadeSheriff sheriffChosen = null;
		bool direction = false;
		sheriffs = new List<RetroArcadeSheriff>();
		for (int i = 0; i < spawnPositions.Count; i++)
		{
			colorString2 = p.colorString[colorMainIndex].Split(',');
			if (colorString2[colorIndex][0] == 'G')
			{
				sheriffChosen = sheriffGreenPrefab;
			}
			else if (colorString2[colorIndex][0] == 'Y')
			{
				sheriffChosen = sheriffYellowPrefab;
			}
			else if (colorString2[colorIndex][0] == 'O')
			{
				sheriffChosen = sheriffOrangePrefab;
			}
			RetroArcadeSheriff item = sheriffChosen.Create(spawnPositions[i], p.moveSpeed, direction, 20f, p);
			sheriffs.Add(item);
			if (colorIndex < colorString2.Length - 1)
			{
				colorIndex++;
				continue;
			}
			colorMainIndex = (colorMainIndex + 1) % p.colorString.Length;
			colorIndex = 0;
		}
		yield return CupheadTime.WaitForSeconds(this, 1f);
		foreach (RetroArcadeSheriff sheriff in sheriffs)
		{
			sheriff.StartMoving();
		}
		StartCoroutine(check_if_dead_cr());
		float delay = 0f;
		Parser.FloatTryParse(delayString[delayIndex], out delay);
		yield return CupheadTime.WaitForSeconds(this, delay - delaySubstract);
		while (true)
		{
			int chosen = Random.Range(0, sheriffs.Count);
			int countDeadOnes = 0;
			for (int j = 0; j < sheriffs.Count; j++)
			{
				if (sheriffs[j].IsDead)
				{
					countDeadOnes++;
				}
			}
			if (countDeadOnes >= sheriffs.Count)
			{
				break;
			}
			while (sheriffs[chosen].IsDead)
			{
				chosen = Random.Range(0, sheriffs.Count);
				yield return null;
			}
			AbstractPlayerController player = PlayerManager.GetNext();
			sheriffs[chosen].Shoot(player);
			if (delayIndex < delayString.Length - 1)
			{
				delayIndex++;
			}
			else
			{
				delayMainIndex = (delayMainIndex + 1) % p.delayString.Length;
				delayIndex = 0;
			}
			yield return null;
			Parser.FloatTryParse(delayString[delayIndex], out delay);
			yield return CupheadTime.WaitForSeconds(this, delay - delaySubstract);
		}
		EndPhase();
		yield return null;
	}

	private IEnumerator check_if_dead_cr()
	{
		bool[] killedOff = new bool[sheriffs.Count];
		while (true)
		{
			for (int i = 0; i < sheriffs.Count; i++)
			{
				if (sheriffs[i].IsDead && !killedOff[i])
				{
					killedOff[i] = true;
					HandleDeathChange();
				}
			}
			yield return null;
		}
	}

	private void HandleDeathChange()
	{
		for (int i = 0; i < sheriffs.Count; i++)
		{
			sheriffs[i].speed += base.properties.CurrentState.sheriff.moveSpeedAddition;
		}
		delaySubstract += base.properties.CurrentState.sheriff.delayMinus;
	}

	private void EndPhase()
	{
		StopAllCoroutines();
		foreach (RetroArcadeSheriff sheriff in sheriffs)
		{
			Object.Destroy(sheriff.gameObject);
		}
		base.properties.DealDamageToNextNamedState();
	}
}
