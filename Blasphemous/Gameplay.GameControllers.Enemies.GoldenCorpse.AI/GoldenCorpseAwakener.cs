using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.GoldenCorpse.AI;

public class GoldenCorpseAwakener : MonoBehaviour
{
	public List<GoldenCorpseBehaviour> allCorpses;

	public List<GoldenCorpseBehaviour> awakenedOnes;

	public List<GoldenCorpseBehaviour> asleepOnes;

	[Button("GetAllCorpses", ButtonSizes.Small)]
	public void GetCorpsesInScene()
	{
		allCorpses = new List<GoldenCorpseBehaviour>(Object.FindObjectsOfType<GoldenCorpseBehaviour>());
	}

	private void Start()
	{
		awakenedOnes = new List<GoldenCorpseBehaviour>();
		asleepOnes = new List<GoldenCorpseBehaviour>();
		asleepOnes.AddRange(allCorpses);
	}

	private void AwakenCorpse(GoldenCorpseBehaviour c)
	{
		c.Awaken();
		awakenedOnes.Add(c);
		asleepOnes.Remove(c);
	}

	private void BackToSleep(GoldenCorpseBehaviour c)
	{
		c.SleepForever();
		awakenedOnes.Remove(c);
		asleepOnes.Add(c);
	}

	public void AllBackToSleep()
	{
		foreach (GoldenCorpseBehaviour awakenedOne in awakenedOnes)
		{
			awakenedOne.SleepForever();
		}
	}

	[Button("Awaken Random Corpse", ButtonSizes.Small)]
	public void AwakenRandomCorpse()
	{
		GoldenCorpseBehaviour randomAsleepCorpse = GetRandomAsleepCorpse();
		if (randomAsleepCorpse != null)
		{
			AwakenCorpse(randomAsleepCorpse);
		}
	}

	private GoldenCorpseBehaviour GetRandomAsleepCorpse()
	{
		if (asleepOnes.Count > 0)
		{
			return asleepOnes[Random.Range(0, asleepOnes.Count)];
		}
		Debug.Log("NOT ASLEEP ONES REMAIN");
		return null;
	}
}
