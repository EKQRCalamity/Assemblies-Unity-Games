using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroArcadeChaserManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private RetroArcadeChaser chaserGreenPrefab;

	[SerializeField]
	private RetroArcadeChaser chaserYellowPrefab;

	[SerializeField]
	private RetroArcadeChaser chaserOrangePrefab;

	private Vector3[] spawnPositions;

	private List<RetroArcadeChaser> chasers;

	public void StartChasers()
	{
		SetupSpawnPoints();
		StartCoroutine(chasers_cr());
	}

	private void SetupSpawnPoints()
	{
		int num = 8;
		float num2 = Level.Current.Right / (num - 1) * 2;
		float num3 = 5f;
		int num4 = 0;
		spawnPositions = new Vector3[num * 2];
		for (int i = 0; i < num * 2; i++)
		{
			float x = (float)Level.Current.Left + num2 * (float)num4;
			float y = ((i >= num) ? ((float)Level.Current.Ground + num3) : ((float)Level.Current.Ceiling - num3));
			ref Vector3 reference = ref spawnPositions[i];
			reference = new Vector3(x, y);
			num4 = ((i != num - 1) ? (++num4) : 0);
		}
	}

	private IEnumerator chasers_cr()
	{
		LevelProperties.RetroArcade.Chasers p = base.properties.CurrentState.chasers;
		int mainColorIndex = Random.Range(0, p.colorString.Length);
		string[] colorString = p.colorString[mainColorIndex].Split(',');
		int mainDelayIndex = Random.Range(0, p.delayString.Length);
		string[] delayString2 = p.delayString[mainDelayIndex].Split(',');
		int delayIndex = Random.Range(0, delayString2.Length);
		int mainOrderIndex = Random.Range(0, p.orderString.Length);
		string[] orderString2 = p.orderString[mainOrderIndex].Split(',');
		int orderIndex = Random.Range(0, orderString2.Length);
		RetroArcadeChaser chaserSelected = null;
		int spawnIndex = 0;
		float delay = 0f;
		float chaserSpeed = 0f;
		float chaserrotation = 0f;
		float chaserHp = 0f;
		float chaserDuration = 0f;
		chasers = new List<RetroArcadeChaser>();
		for (int i = 0; i < colorString.Length; i++)
		{
			AbstractPlayerController player = PlayerManager.GetNext();
			orderString2 = p.orderString[mainOrderIndex].Split(',');
			delayString2 = p.delayString[mainDelayIndex].Split(',');
			if (colorString[i][0] == 'G')
			{
				chaserSelected = chaserGreenPrefab;
				chaserSpeed = p.greenSpeed;
				chaserrotation = p.greenRotation;
				chaserHp = p.greenHP;
				chaserDuration = p.greenDuration;
			}
			else if (colorString[i][0] == 'Y')
			{
				chaserSelected = chaserYellowPrefab;
				chaserSpeed = p.yellowSpeed;
				chaserrotation = p.yellowRotation;
				chaserHp = p.yellowHP;
				chaserDuration = p.yellowDuration;
			}
			else if (colorString[i][0] == 'O')
			{
				chaserSelected = chaserOrangePrefab;
				chaserSpeed = p.orangeSpeed;
				chaserrotation = p.orangeRotation;
				chaserHp = p.orangeHP;
				chaserDuration = p.orangeDuration;
			}
			Parser.IntTryParse(orderString2[orderIndex], out spawnIndex);
			Parser.FloatTryParse(delayString2[delayIndex], out delay);
			RetroArcadeChaser chaser = chaserSelected.Spawn();
			chaser.Init(spawnPositions[spawnIndex], 0f, chaserSpeed, chaserSpeed, chaserrotation, chaserDuration, chaserHp, player, p);
			chasers.Add(chaser);
			if (orderIndex < p.orderString.Length - 1)
			{
				orderIndex++;
			}
			else
			{
				mainOrderIndex = (mainOrderIndex + 1) % p.orderString.Length;
				orderIndex = 0;
			}
			if (delayIndex < p.delayString.Length - 1)
			{
				delayIndex++;
			}
			else
			{
				mainDelayIndex = (mainDelayIndex + 1) % p.delayString.Length;
				delayIndex = 0;
			}
			yield return CupheadTime.WaitForSeconds(this, delay);
		}
		bool isDone = false;
		while (!isDone)
		{
			isDone = true;
			foreach (RetroArcadeChaser chaser2 in chasers)
			{
				isDone = chaser2.IsDone;
			}
			yield return null;
		}
		foreach (RetroArcadeChaser chaser3 in chasers)
		{
			Object.Destroy(chaser3);
		}
		base.properties.DealDamageToNextNamedState();
		yield return null;
	}
}
