using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SnakeBackgroundAnimator : MonoBehaviour
{
	public List<Animator> animators;

	private float currentSpeed = 1f;

	private Tween currentTween;

	private void Awake()
	{
		Activate(active: false);
	}

	public void Activate(bool active)
	{
		LerpSpeed(active ? 1 : 0);
	}

	public void SetAnimatorsSpeed(float spd)
	{
		currentSpeed = spd;
		foreach (Animator animator in animators)
		{
			animator.SetFloat("SPEED", currentSpeed);
		}
	}

	public float GetAnimatorSpeed()
	{
		return currentSpeed;
	}

	public void LerpSpeed(float spd, float seconds = 0.5f)
	{
		if (currentTween != null && currentTween.IsPlaying())
		{
			currentTween.Kill();
		}
		currentTween = DOTween.To(GetAnimatorSpeed, SetAnimatorsSpeed, spd, seconds).SetUpdate(UpdateType.Normal, isIndependentUpdate: false).SetEase(Ease.InQuad);
	}
}
