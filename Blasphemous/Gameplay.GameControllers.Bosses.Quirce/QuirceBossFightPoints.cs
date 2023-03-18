using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Quirce;

public class QuirceBossFightPoints : MonoBehaviour
{
	public enum QUIRCE_FIGHT_SIDES
	{
		LEFT,
		RIGHT
	}

	public GameObject wallSwordMask;

	public List<SplinePointInfo> ceilingHangPoints;

	public List<SplinePointInfo> startDashPoint;

	public List<Transform> teleportPlungePoints;

	public List<Transform> spiralPoint;

	public SplinePointInfo spiralPointInfo;

	public Transform roomCenter;

	public Transform swordWallPoint;

	public Transform GetSwordWallPoint()
	{
		return swordWallPoint;
	}

	public Transform GetTeleportPlungeTransform()
	{
		return teleportPlungePoints[Random.Range(0, teleportPlungePoints.Count)];
	}

	public Transform GetHangTransform(List<QUIRCE_FIGHT_SIDES> valid)
	{
		List<SplinePointInfo> list = ceilingHangPoints.FindAll((SplinePointInfo x) => valid.Contains(x.fightSide));
		list.RemoveAll((SplinePointInfo x) => !x.active);
		return list[Random.Range(0, list.Count)].point;
	}

	public SplinePointInfo GetHangPointInfo(Transform t)
	{
		return ceilingHangPoints.Find((SplinePointInfo x) => x.point == t);
	}

	public Transform GetDashPointTransform(List<QUIRCE_FIGHT_SIDES> valid)
	{
		List<SplinePointInfo> list = startDashPoint.FindAll((SplinePointInfo x) => valid.Contains(x.fightSide));
		list.RemoveAll((SplinePointInfo x) => !x.active);
		return list[Random.Range(0, list.Count)].point;
	}

	public SplinePointInfo GetDashPointInfo(Transform t)
	{
		return startDashPoint.Find((SplinePointInfo x) => x.point == t);
	}

	public Vector3 GetCenter()
	{
		return roomCenter.position;
	}

	public Vector3 GetTossPoint()
	{
		return spiralPoint[Random.Range(0, spiralPoint.Count)].position;
	}

	public void ActivateWallMask(bool v)
	{
		wallSwordMask.SetActive(v);
	}
}
