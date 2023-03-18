using System.Collections.Generic;
using DG.Tweening;
using Gameplay.GameControllers.Bosses.Quirce;
using UnityEngine;

public class AnguishBossfightConfig : MonoBehaviour
{
	public SplinePointInfo maceSpline;

	public List<Transform> beamPoints;

	public List<Transform> spearComboPointsL;

	public List<Transform> spearComboPointsR;

	public Transform flameWall;

	public void ShowFlameWall()
	{
		flameWall.DOLocalMoveY(flameWall.localPosition.y + 2.5f, 4f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			flameWall.DOLocalMoveY(flameWall.localPosition.y - 1f, 2f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
		});
	}

	public void HideFlameWall()
	{
		flameWall.DOKill();
		flameWall.DOLocalMoveY(flameWall.localPosition.y - 3f, 3f).SetEase(Ease.InOutQuad);
		flameWall.GetComponentInParent<Collider2D>().enabled = false;
	}

	public Transform GetRandomBeamPoint()
	{
		return beamPoints[Random.Range(0, beamPoints.Count)];
	}

	public Transform GetDifferentBeamTransform(Transform currentTransform)
	{
		Transform transform = currentTransform;
		while (transform == currentTransform)
		{
			transform = GetRandomBeamPoint();
		}
		return transform;
	}

	public SplinePointInfo GetMaceSplineInfo()
	{
		return maceSpline;
	}

	public List<Transform> GetSpearPoints()
	{
		if (Random.Range(0f, 1f) > 0.5f)
		{
			return spearComboPointsL;
		}
		return spearComboPointsR;
	}
}
