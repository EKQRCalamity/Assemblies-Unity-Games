using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetroArcadeTrafficManager : LevelProperties.RetroArcade.Entity
{
	[SerializeField]
	private RetroArcadeTrafficUFO trafficUFO;

	[SerializeField]
	private GameObject trafficLightPrefab;

	private const int GRIDX = 4;

	private const int GRIDY = 4;

	private GameObject[,] trafficGrid;

	private List<Vector3> positionsToTravel;

	public void StartTraffic()
	{
		SpawnTrafficLights();
		StartUFO();
		StartCoroutine(move_ufo_cr());
	}

	public void StartUFO()
	{
		trafficUFO.gameObject.SetActive(value: true);
		AbstractPlayerController next = PlayerManager.GetNext();
		if (next.transform.position.x > 0f)
		{
			trafficUFO.transform.position = trafficGrid[0, 3].transform.position;
		}
		else
		{
			trafficUFO.transform.position = trafficGrid[3, 3].transform.position;
		}
	}

	private void SpawnTrafficLights()
	{
		trafficGrid = new GameObject[4, 4];
		float num = Level.Current.Width / 4;
		float num2 = Level.Current.Height / 4;
		Vector3 vector = new Vector3((float)Level.Current.Left + num / 2f, (float)Level.Current.Ground + num2 / 2f);
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector2 = new Vector3((float)i * num, (float)j * num2);
				GameObject gameObject = Object.Instantiate(trafficLightPrefab);
				trafficGrid[i, j] = gameObject;
				trafficGrid[i, j].transform.position = vector + vector2;
				trafficGrid[i, j].transform.parent = base.transform;
			}
		}
	}

	private IEnumerator move_ufo_cr()
	{
		LevelProperties.RetroArcade.Traffic p = base.properties.CurrentState.traffic;
		int lightMainIndex = Random.Range(0, p.lightOrderString.Length);
		string[] lightString2 = p.lightOrderString[lightMainIndex].Split(',');
		int lightX = 0;
		int lightY = 0;
		positionsToTravel = new List<Vector3>();
		while (!trafficUFO.IsDead)
		{
			lightString2 = p.lightOrderString[lightMainIndex].Split(',');
			for (int i = 0; i < lightString2.Length; i++)
			{
				yield return CupheadTime.WaitForSeconds(this, p.lightDelay);
				string getLightCoordX = lightString2[i].Substring(1);
				string getLightCoordY = lightString2[i].Substring(0, 1);
				Parser.IntTryParse(getLightCoordX, out lightX);
				switch (getLightCoordY)
				{
				case "A":
					lightY = 0;
					break;
				case "B":
					lightY = 1;
					break;
				case "C":
					lightY = 2;
					break;
				case "D":
					lightY = 3;
					break;
				}
				trafficGrid[lightX, lightY].GetComponent<Animator>().SetBool("IsGreen", value: true);
				positionsToTravel.Add(trafficGrid[lightX, lightY].transform.position);
			}
			trafficUFO.StartMoving(positionsToTravel, p.moveSpeed, p.moveDelay);
			while (trafficUFO.IsMoving && !trafficUFO.IsDead)
			{
				yield return null;
			}
			ResetLights();
			positionsToTravel.Clear();
			lightMainIndex = (lightMainIndex + 1) % p.lightOrderString.Length;
			yield return null;
		}
		EndPhase();
	}

	private void EndPhase()
	{
		StopAllCoroutines();
		DestroyLights();
		Object.Destroy(trafficUFO.gameObject);
		base.properties.DealDamageToNextNamedState();
	}

	private void ResetLights()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				trafficGrid[i, j].GetComponent<Animator>().SetBool("IsGreen", value: false);
			}
		}
	}

	private void DestroyLights()
	{
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Object.Destroy(trafficGrid[i, j]);
			}
		}
	}
}
