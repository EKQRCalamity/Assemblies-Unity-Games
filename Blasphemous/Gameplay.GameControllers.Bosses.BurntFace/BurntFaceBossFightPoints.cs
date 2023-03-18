using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace;

public class BurntFaceBossFightPoints : MonoBehaviour
{
	public List<PointsByPatternId> rosaryPoints;

	public List<PointsByPatternId> headPoints;

	public Transform nunPoint;

	private Transform _lastRosaryPoint;

	public Transform GetNunPoint()
	{
		return nunPoint;
	}

	public Transform GetHeadPoint(string id)
	{
		List<Transform> points = headPoints.Find((PointsByPatternId x) => x.id == id).points;
		return points[Random.Range(0, points.Count)];
	}

	public Transform GetRosaryPoint(string id, bool secondary = false)
	{
		if (secondary)
		{
			id = string.Format("{0}_{1}", id, "SEC");
		}
		List<Transform> list = new List<Transform>(rosaryPoints.Find((PointsByPatternId x) => x.id == id).points);
		list.Remove(_lastRosaryPoint);
		return _lastRosaryPoint = list[Random.Range(0, list.Count)];
	}
}
