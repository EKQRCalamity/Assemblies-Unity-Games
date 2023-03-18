using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses;

public class BossPlayerAwareness : MonoBehaviour
{
	public List<BossAwarenessArea> awarenessAreas;

	public ContactFilter2D playerFilter;

	private Collider2D[] results;

	private void Start()
	{
		results = new Collider2D[1];
	}

	private void Update()
	{
		CheckAwarenessAreas();
	}

	private void CheckAwarenessAreas()
	{
		foreach (BossAwarenessArea awarenessArea in awarenessAreas)
		{
			UpdateArea(awarenessArea);
		}
	}

	public bool AreaContainsPlayer(string id)
	{
		return awarenessAreas.Find((BossAwarenessArea x) => x.id == id).containsPlayer;
	}

	private void UpdateArea(BossAwarenessArea a)
	{
		a.containsPlayer = a.area.OverlapCollider(playerFilter, results) > 0;
	}
}
