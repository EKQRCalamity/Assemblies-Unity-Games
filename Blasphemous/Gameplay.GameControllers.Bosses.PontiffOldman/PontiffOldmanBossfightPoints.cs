using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffOldman;

public class PontiffOldmanBossfightPoints : MonoBehaviour
{
	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform toxicPointsParent;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform leftLimitTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform rightLimitTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	public Transform fightCenterTransform;

	[SerializeField]
	[FoldoutGroup("References", 0)]
	private Transform repositionPointsParent;

	public List<Transform> repositionPoints;

	public List<Transform> midPoints;

	[Button("Load all lists", ButtonSizes.Large)]
	public void LoadListsFromParents()
	{
		repositionPoints.Clear();
		foreach (Transform item in repositionPointsParent)
		{
			repositionPoints.Add(item);
		}
	}

	public Transform GetRandomToxicPoint()
	{
		int childCount = toxicPointsParent.childCount;
		int index = Random.Range(0, childCount);
		return toxicPointsParent.GetChild(index);
	}

	public Transform GetPointInCenter()
	{
		return midPoints[Random.Range(0, midPoints.Count)];
	}

	public Transform GetPointAwayOfPenitent(Vector2 p_position)
	{
		Transform transform = repositionPoints[0];
		float num = Vector2.Distance(transform.position, p_position);
		for (int i = 1; i < repositionPoints.Count; i++)
		{
			float num2 = Vector2.Distance(repositionPoints[i].position, p_position);
			if (num2 > num)
			{
				transform = repositionPoints[i];
				num = num2;
			}
		}
		return transform;
	}
}
