using System.Collections;
using UnityEngine;

public class MapCastleZones : MonoBehaviour
{
	public enum Zone
	{
		None,
		OldMan,
		RumRunners,
		Cowgirl,
		DogFight,
		SnowCult,
		Dock
	}

	private static readonly Zone[] RegularZones = new Zone[5]
	{
		Zone.OldMan,
		Zone.RumRunners,
		Zone.Cowgirl,
		Zone.DogFight,
		Zone.SnowCult
	};

	[SerializeField]
	private MapCastleZoneCollider[] zones;

	[SerializeField]
	private MapLevelLoaderLadder ladder;

	private MapCastleZoneCollider currentZone;

	private int currentZonePlayerCount;

	private void OnEnable()
	{
		MapCastleZoneCollider[] array = zones;
		foreach (MapCastleZoneCollider mapCastleZoneCollider in array)
		{
			mapCastleZoneCollider.OnMapCastleZoneCollision += onMapCastleZoneCollision;
		}
	}

	private void OnDisable()
	{
		MapCastleZoneCollider[] array = zones;
		foreach (MapCastleZoneCollider mapCastleZoneCollider in array)
		{
			mapCastleZoneCollider.OnMapCastleZoneCollision -= onMapCastleZoneCollision;
		}
	}

	private void showLadder(MapCastleZoneCollider zone)
	{
		ladder.transform.position = zone.interactionPoint.position;
		ladder.returnPositions = zone.returnPositions;
		StartCoroutine(showLadder_cr(zone));
	}

	private IEnumerator showLadder_cr(MapCastleZoneCollider zone)
	{
		ladder.animator.SetBool("Down", value: true);
		yield return ladder.animator.WaitForAnimationToStart(this, "Drop");
		AudioManager.Play("worldmap_kog_ladder_down");
		ladder.EnableShadow(zone.enableLadderShadow);
		yield return ladder.animator.WaitForAnimationToEnd(this, "Drop");
		ladder.enabled = true;
	}

	private void hideLadder()
	{
		StartCoroutine(hideLadder_cr());
	}

	private IEnumerator hideLadder_cr()
	{
		ladder.animator.SetBool("Down", value: false);
		yield return ladder.animator.WaitForAnimationToStart(this, "Up");
		AudioManager.Play("worldmap_kog_ladder_up");
		ladder.enabled = false;
	}

	private void onMapCastleZoneCollision(MapCastleZoneCollider collider, GameObject other, CollisionPhase phase)
	{
		switch (phase)
		{
		case CollisionPhase.Enter:
			if (currentZone == null)
			{
				PlayerData data = PlayerData.Data;
				if (data.CountLevelsCompleted(Level.kingOfGamesLevels) == Level.kingOfGamesLevels.Length)
				{
					if (collider.zone == Zone.Dock)
					{
						currentZone = collider;
					}
				}
				else if (collider.zone != Zone.Dock)
				{
					if (data.currentChessBossZone == Zone.None)
					{
						int num = data.CountLevelsCompleted(Level.worldDLCBossLevels);
						int count = data.usedChessBossZones.Count;
						if (count <= num && !data.usedChessBossZones.Contains(collider.zone))
						{
							currentZone = collider;
							data.currentChessBossZone = collider.zone;
							PlayerData.SaveCurrentFile();
						}
					}
					else if (data.currentChessBossZone == collider.zone)
					{
						currentZone = collider;
					}
				}
				if (currentZone != null)
				{
					showLadder(currentZone);
				}
			}
			if (collider == currentZone)
			{
				currentZonePlayerCount++;
			}
			break;
		case CollisionPhase.Exit:
			if (collider == currentZone)
			{
				currentZonePlayerCount--;
				if (currentZonePlayerCount == 0)
				{
					currentZone = null;
					hideLadder();
				}
			}
			break;
		}
	}
}
