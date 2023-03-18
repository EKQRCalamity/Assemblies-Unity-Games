using System;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities.Animations;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PontiffGiant.Animator;

public class PontiffGiantAnimatorInyector : EnemyAnimatorInyector
{
	public UnityEngine.Animator headAnimator;

	public UnityEngine.Animator faceAnimator;

	public UnityEngine.Animator bodyAnimator;

	public Gradient introGradient;

	public float introBlendDuration = 2f;

	public event Action<PontiffGiantAnimatorInyector, Vector2> OnSpinProjectilePoint;

	public void AnimationEvent_LightScreenShake()
	{
		Vector2 vector = ((OwnerEntity.Status.Orientation != 0) ? Vector2.left : Vector2.right);
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.15f, vector * 0.5f, 10, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void AnimationEvent_HeavyScreenShake()
	{
		Core.Logic.CameraManager.ProCamera2DShake.Shake(0.3f, Vector3.up * 3f, 60, 0.01f, 0f, default(Vector3), 0.01f);
	}

	public void IntroBlend()
	{
		SpriteRenderer component = headAnimator.GetComponent<SpriteRenderer>();
		SpriteRenderer component2 = faceAnimator.GetComponent<SpriteRenderer>();
		SpriteRenderer component3 = bodyAnimator.GetComponent<SpriteRenderer>();
		component.DOGradientColor(introGradient, introBlendDuration);
		component2.DOGradientColor(introGradient, introBlendDuration);
		component3.DOGradientColor(introGradient, introBlendDuration);
	}

	public void Death()
	{
		faceAnimator.SetTrigger("DEATH");
	}

	public void Open(bool open)
	{
		headAnimator.SetBool("OPEN", open);
		faceAnimator.SetBool("OPEN", open);
		bodyAnimator.SetBool("OPEN", open);
	}
}
