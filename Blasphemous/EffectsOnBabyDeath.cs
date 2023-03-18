using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EffectsOnBabyDeath : MonoBehaviour
{
	public List<Transform> flameWalls;

	public GameObject effectsParent;

	public void ActivateEffects()
	{
		effectsParent.SetActive(value: true);
		foreach (Transform flameWall in flameWalls)
		{
			ShowFlameWall(flameWall);
		}
	}

	public void ShowFlameWall(Transform flameWall)
	{
		flameWall.GetComponent<SpriteRenderer>().DOFade(1f, 0.5f);
		float r = Random.Range(1.5f, 3f);
		flameWall.DOLocalMoveY(flameWall.localPosition.y + 2f, 4f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			flameWall.DOLocalMoveY(flameWall.localPosition.y - 1f, r).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
		});
	}

	public void HideFlameWall(Transform flameWall)
	{
		flameWall.DOKill();
		flameWall.DOLocalMoveY(flameWall.localPosition.y - 3f, 3f).SetEase(Ease.InOutQuad);
		flameWall.GetComponentInParent<Collider2D>().enabled = false;
	}
}
