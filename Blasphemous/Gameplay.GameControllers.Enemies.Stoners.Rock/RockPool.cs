using System.Collections.Generic;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Rock;

public class RockPool : MonoBehaviour
{
	public Enemy EnemyOwner;

	public GameObject Rock;

	private readonly List<GameObject> Rocks = new List<GameObject>();

	public GameObject GetRock(Vector3 position)
	{
		GameObject gameObject = null;
		if (Rocks.Count > 0)
		{
			gameObject = Rocks[Rocks.Count - 1];
			Rocks.Remove(gameObject);
			gameObject.SetActive(value: true);
			gameObject.transform.position = position;
			gameObject.transform.rotation = Quaternion.identity;
		}
		else
		{
			gameObject = Object.Instantiate(Rock, position, Quaternion.identity);
		}
		StonersRock componentInChildren = gameObject.GetComponentInChildren<StonersRock>();
		componentInChildren.AttackArea.Entity = EnemyOwner;
		return gameObject;
	}

	public void StoreRock(GameObject rock)
	{
		if (!Rocks.Contains(rock))
		{
			Rocks.Add(rock);
			rock.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if (Rocks.Count > 0)
		{
			Rocks.Clear();
		}
	}
}
