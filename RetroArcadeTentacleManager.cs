using System.Collections;
using UnityEngine;

public class RetroArcadeTentacleManager : LevelProperties.RetroArcade.Entity
{
	private const float LEFT_SIDE_SPAWN = -320f;

	private const float RIGHT_SIDE_SPAWN = 320f;

	private const int SPACES_COUNT = 8;

	private const float SPAWN_OFFSET = 240f;

	[SerializeField]
	private GameObject octopusHead;

	[SerializeField]
	private RetroArcadeTentacle tentaclePrefab;

	[SerializeField]
	private Transform bottom;

	private RetroArcadeTentacle[] tentacles;

	private float[] YSpawnPoints;

	private float offset;

	public void StartTentacle()
	{
		SetupYSpawnpoints();
		StartCoroutine(spawn_tentacles_cr());
	}

	private void SetupYSpawnpoints()
	{
		YSpawnPoints = new float[8];
		float num = (float)Level.Current.Ground - bottom.position.y;
		offset = ((float)Level.Current.Height - num) / 8f;
		for (int i = 0; i < 8; i++)
		{
			float num2 = offset * (float)i;
			YSpawnPoints[i] = num2;
		}
	}

	private IEnumerator spawn_tentacles_cr()
	{
		octopusHead.SetActive(value: true);
		LevelProperties.RetroArcade.Tentacle p = base.properties.CurrentState.tentacle;
		int mainTargetIndex = Random.Range(0, p.targetString.Length);
		string[] targetString = p.targetString[mainTargetIndex].Split(',');
		int targetIndex = Random.Range(0, targetString.Length);
		int counter = 0;
		int positionIndex = 0;
		bool spawningLeft = Rand.Bool();
		RetroArcadeTentacle tentacle2 = null;
		RetroArcadeTentacle lastLeftTentacle = null;
		RetroArcadeTentacle lastRightTentacle = null;
		tentacles = new RetroArcadeTentacle[p.tentacleCount];
		while (counter < p.tentacleCount)
		{
			targetString = p.targetString[mainTargetIndex].Split(',');
			Parser.IntTryParse(targetString[targetIndex], out positionIndex);
			Vector3 spawnPoint = new Vector3((!spawningLeft) ? 320f : (-320f), -500f);
			if (spawningLeft)
			{
				while (lastLeftTentacle != null && !(lastLeftTentacle.transform.position.x >= -240f))
				{
					yield return null;
				}
			}
			else
			{
				while (lastRightTentacle != null && !(lastRightTentacle.transform.position.x <= 240f))
				{
					yield return null;
				}
			}
			tentacle2 = tentaclePrefab.Spawn();
			tentacle2.Init(spawnPoint, YSpawnPoints[positionIndex], spawningLeft, p.risingSpeed, p.moveSpeed);
			tentacles[counter] = tentacle2;
			if (spawningLeft)
			{
				lastLeftTentacle = tentacle2;
			}
			else
			{
				lastRightTentacle = tentacle2;
			}
			if (targetIndex < targetString.Length - 1)
			{
				targetIndex++;
			}
			else
			{
				mainTargetIndex = (mainTargetIndex + 1) % p.targetString.Length;
				targetIndex = 0;
			}
			spawningLeft = !spawningLeft;
			counter++;
			yield return null;
		}
		int countDeadOnes2 = 0;
		while (true)
		{
			countDeadOnes2 = 0;
			for (int i = 0; i < tentacles.Length; i++)
			{
				if (tentacles[i] == null)
				{
					countDeadOnes2++;
				}
			}
			if (countDeadOnes2 >= tentacles.Length)
			{
				break;
			}
			yield return null;
		}
		octopusHead.SetActive(value: false);
		base.properties.DealDamageToNextNamedState();
		yield return null;
	}
}
