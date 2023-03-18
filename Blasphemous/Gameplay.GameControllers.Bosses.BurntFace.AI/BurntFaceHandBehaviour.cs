using System;
using DG.Tweening;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BurntFace.AI;

public class BurntFaceHandBehaviour : MonoBehaviour
{
	public SpriteRenderer spr;

	public Animator muzzleFlashAnimator;

	[FoldoutGroup("Attack references", 0)]
	public BurntFaceRosaryManager rosary;

	[FoldoutGroup("Attack references", 0)]
	public BossHomingLaserAttack targetedBeamAttack;

	[FoldoutGroup("Attack references", 0)]
	public BossStraightProjectileAttack homingBallsLauncher;

	public void ClearAll()
	{
		base.transform.DOKill();
		SetMuzzleFlash(on: false);
		rosary.Clear();
		homingBallsLauncher.Clear();
		targetedBeamAttack.Clear();
		rosary.gameObject.SetActive(value: false);
		homingBallsLauncher.gameObject.SetActive(value: false);
		targetedBeamAttack.gameObject.SetActive(value: false);
	}

	private void Start()
	{
		rosary.RegenerateAllBeads();
	}

	public void SetMuzzleFlash(bool on)
	{
		muzzleFlashAnimator.SetBool("ACTIVE", on);
	}

	public void MoveToPosition(Vector2 pos, float seconds, Action<BurntFaceHandBehaviour> callback)
	{
		base.transform.DOMove(pos, seconds).OnComplete(delegate
		{
			callback(this);
		}).SetEase(Ease.InOutBack);
	}

	public void SetFlipX(bool flip)
	{
		spr.flipX = flip;
	}

	public void Show(float seconds)
	{
		spr.DOFade(1f, seconds);
	}

	public void Hide(float seconds)
	{
		SetMuzzleFlash(on: false);
		spr.DOFade(0f, seconds);
	}
}
